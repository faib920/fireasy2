using Fireasy.Common;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using Fireasy.Common.Subscribes;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Fireasy.RabbitMQ
{
    [ConfigurationSetting(typeof(RabbitConfigurationSetting))]
    public class RabbitSubscribeManager : ISubscribeManager, IConfigurationSettingHostService
    {
        private RabbitConfigurationSetting setting;

        private static IConnection connection;
        private static ConcurrentDictionary<Type, List<Delegate>> subscribers = new ConcurrentDictionary<Type, List<Delegate>>();
        private static ConcurrentDictionary<Type, IModel> channels = new ConcurrentDictionary<Type, IModel>();

        /// <summary>
        /// 添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber<TSubject>(Action<TSubject> subscriber) where TSubject : ISubject
        {
            AddSubscriber(typeof(TSubject), subscriber);
        }

        /// <summary>
        /// 添加一个订阅方法。
        /// </summary>
        /// <param name="subjectType">主题的类型。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber(Type subjectType, Delegate subscriber)
        {
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            var list = subscribers.GetOrAdd(subjectType, k =>
                {
                    StartQueue(subjectType);
                    return new List<Delegate>();
                });

            list.Add(subscriber);
        }

        public void Publish<TSubject>(TSubject subject) where TSubject : ISubject
        {
            var queueName = typeof(TSubject).FullName;

            using (var model = connection.CreateModel())
            {
                model.QueueDeclare(queueName, true, false, false, null);

                var properties = model.CreateBasicProperties();
                properties.Persistent = true;
                properties.DeliveryMode = 2;

                var msgBody = Encoding.UTF8.GetBytes(Serialize(subject));

                model.BasicPublish(string.Empty, queueName, properties, msgBody);
            }
        }

        /// <summary>
        /// 序列化对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual T Deserialize<T>(string value)
        {
            var serializer = CreateSerializer();
            return serializer.Deserialize<T>(value);
        }

        /// <summary>
        /// 序列化对象。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual object Deserialize(Type type, string value)
        {
            var serializer = CreateSerializer();
            return serializer.Deserialize(type, value);
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual string Serialize<T>(T value)
        {
            var serializer = CreateSerializer();
            return serializer.Serialize(value);
        }

        private RabbitSerializer CreateSerializer()
        {
            if (setting.SerializerType != null)
            {
                var serializer = setting.SerializerType.New<RabbitSerializer>();
                if (serializer != null)
                {
                    return serializer;
                }
            }

            return new RabbitSerializer();
        }

        private IConnection GetConnection()
        {
            if (connection == null)
            {
                var factory = new ConnectionFactory
                    {
                        UserName = setting.UserName,
                        Password = setting.Password,
                        Endpoint = new AmqpTcpEndpoint(new Uri(setting.Server)),
                        RequestedHeartbeat = 12,
                        AutomaticRecoveryEnabled = true
                    };

                connection = factory.CreateConnection();
            }

            return connection;
        }

        private void StartQueue(Type subjectType)
        {
            var queueName = subjectType.FullName;

            var model = channels.GetOrAdd(subjectType, k => connection.CreateModel());
            var queue = model.QueueDeclare(queueName, true, false, false, null);

            //公平分发,不要同一时间给一个工作者发送多于一个消息
            model.BasicQos(0, 1, false);

            //创建事件驱动的消费者类型，不要用下边的死循环来消费消息
            var consumer = new EventingBasicConsumer(model);
            consumer.Received += (sender, args) =>
                {
                    if (subscribers.TryGetValue(subjectType, out List<Delegate> list))
                    {
                        var message = Encoding.UTF8.GetString(args.Body);
                        var subject = Deserialize(subjectType, message);

                        list.ForEach(s => s.DynamicInvoke(subject));
                    }
                };

            //消费消息
            model.BasicConsume(queueName, false, consumer);
        }


        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            this.setting = (RabbitConfigurationSetting)setting;
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return setting;
        }
    }
}
