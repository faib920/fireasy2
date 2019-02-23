// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Subscribes;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fireasy.RabbitMQ
{
    [ConfigurationSetting(typeof(RabbitConfigurationSetting))]
    public class SubscribeManager : ISubscribeManager, IConfigurationSettingHostService
    {
        private RabbitConfigurationSetting setting;

        private static Lazy<IConnection> connectionLazy;
        private static SafetyDictionary<string, List<Delegate>> subscribers = new SafetyDictionary<string, List<Delegate>>();
        private static SafetyDictionary<string, IModel> channels = new SafetyDictionary<string, IModel>();

        /// <summary>
        /// 向 Rabbit 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        public void Publish<TSubject>(TSubject subject) where TSubject : class
        {
            var channelName = ChannelHelper.GetChannelName(typeof(TSubject));
            var data = Encoding.UTF8.GetBytes(Serialize(subject));

            Publish(channelName, data);
        }

        /// <summary>
        /// 向指定的 Rabbit 通道发送数据。
        /// </summary>
        /// <param name="channel">通道名称。</param>
        /// <param name="data">发送的数据。</param>
        public void Publish(string channel, byte[] data)
        {
            using (var model = GetConnection().CreateModel())
            {
                model.QueueDeclare(channel, true, false, true, null);

                var properties = model.CreateBasicProperties();
                properties.Persistent = true;
                properties.DeliveryMode = 2;

                model.BasicPublish(string.Empty, channel, properties, data);
            }
        }

        /// <summary>
        /// 在 Rabbit 服务器中添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber<TSubject>(Action<TSubject> subscriber) where TSubject : class
        {
            AddSubscriber(typeof(TSubject), subscriber);
        }

        /// <summary>
        /// 在 Rabbit 服务器中添加一个订阅方法。
        /// </summary>
        /// <param name="subjectType">主题的类型。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber(Type subjectType, Delegate subscriber)
        {
            var channelName = ChannelHelper.GetChannelName(subjectType);
            var list = subscribers.GetOrAdd(channelName, () =>
                {
                    StartQueue(channelName);
                    return new List<Delegate>();
                });

            list.Add(subscriber);
        }

        /// <summary>
        /// 在 Rabbit 服务器中添加一个订阅方法。
        /// </summary>
        /// <param name="channel">通道名称。</param>
        /// <param name="subscriber">读取数据的方法。</param>
        public void AddSubscriber(string channel, Action<byte[]> subscriber)
        {
            var list = subscribers.GetOrAdd(channel, () =>
                {
                    StartQueue(channel);
                    return new List<Delegate>();
                });

            list.Add(subscriber);
        }

        /// <summary>
        /// 移除相关的订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        public void RemoveSubscriber<TSubject>()
        {
            RemoveSubscriber(typeof(TSubject));
        }

        /// <summary>
        /// 移除相关的订阅方法。
        /// </summary>
        /// <param name="subjectType">主题的类型。</param>
        public void RemoveSubscriber(Type subjectType)
        {
            var channelName = ChannelHelper.GetChannelName(subjectType);
            RemoveSubscriber(channelName);
        }

        /// <summary>
        /// 移除指定通道的订阅方法。
        /// </summary>
        /// <param name="channel">通道名称。</param>
        public void RemoveSubscriber(string channel)
        {
            if (channels.TryGetValue(channel, out IModel model))
            {
                model.QueueDelete(channel);
                subscribers.TryRemove(channel, out List<Delegate> delegates);
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
            if (connectionLazy == null)
            {
                connectionLazy = new Lazy<IConnection>(() => new ConnectionFactory
                {
                    UserName = setting.UserName,
                    Password = setting.Password,
                    Endpoint = new AmqpTcpEndpoint(new Uri(setting.Server)),
                    RequestedHeartbeat = 12,
                    AutomaticRecoveryEnabled = true
                }.CreateConnection());
            }

            return connectionLazy.Value;
        }

        private void StartQueue(string channel)
        {
            var model = channels.GetOrAdd(channel, () => GetConnection().CreateModel());
            var queue = model.QueueDeclare(channel, true, false, true, null);

            //创建事件驱动的消费者类型，不要用下边的死循环来消费消息
            var consumer = new EventingBasicConsumer(model);
            consumer.Received += (sender, args) =>
                {
                    if (subscribers.TryGetValue(channel, out List<Delegate> delegates))
                    {
                        foreach (var dele in delegates)
                        {
                            var actType = dele.GetType();
                            if (!actType.IsGenericType || actType.GetGenericTypeDefinition() != typeof(Action<>))
                            {
                                continue;
                            }

                            var subType = actType.GetGenericArguments()[0];
                            if (subType == typeof(byte[]))
                            {
                                dele.DynamicInvoke(args.Body);
                            }
                            else
                            {
                                var body = Deserialize(subType, Encoding.UTF8.GetString(args.Body));
                                dele.DynamicInvoke(body);
                            }
                        }
                    }
                };

            //消费消息
            model.BasicConsume(channel, false, consumer);
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
