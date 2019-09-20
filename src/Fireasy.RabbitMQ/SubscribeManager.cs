// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Subscribes;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Fireasy.RabbitMQ
{
    [ConfigurationSetting(typeof(RabbitConfigurationSetting))]
    public class SubscribeManager : ISubscribeManager, IConfigurationSettingHostService
    {
        private RabbitConfigurationSetting setting;

        private static Lazy<IConnection> connectionLazy;
        private static SafetyDictionary<string, RabbitChannelCollection> subscribers = new SafetyDictionary<string, RabbitChannelCollection>();

        /// <summary>
        /// 向 Rabbit 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        public void Publish<TSubject>(TSubject subject) where TSubject : class
        {
            var channelName = ChannelHelper.GetChannelName(typeof(TSubject));
            var data = Serialize(subject);

            Publish(channelName, data);
        }

        /// <summary>
        /// 向指定的 Rabbit 通道发送数据。
        /// </summary>
        /// <param name="channelName">通道名称。</param>
        /// <param name="data">发送的数据。</param>
        public void Publish(string channelName, byte[] data)
        {
            using (var channel = GetConnection().CreateModel())
            {
                if (string.IsNullOrEmpty(setting.ExchangeType))
                {
                    channel.QueueDeclare(channelName, true, false, false, null);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.DeliveryMode = 2;

                    channel.BasicPublish(string.Empty, channelName, properties, data);
                }
                else
                {
                    var exchangeName = GetExchangeName(channelName);
                    channel.ExchangeDeclare(exchangeName, setting.ExchangeType);
                    channel.BasicPublish(exchangeName, channelName, null, data);
                }
            }
        }

        /// <summary>
        /// 在 Rabbit 服务器中添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber<TSubject>(Action<TSubject> subscriber) where TSubject : class
        {
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            AddSubscriber(typeof(TSubject), subscriber);
        }

        /// <summary>
        /// 在 Rabbit 服务器中添加一个订阅方法。
        /// </summary>
        /// <param name="subjectType">主题的类型。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber(Type subjectType, Delegate subscriber)
        {
            Guard.ArgumentNull(subjectType, nameof(subjectType));
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            var channelName = ChannelHelper.GetChannelName(subjectType);
            var list = subscribers.GetOrAdd(channelName, () => new RabbitChannelCollection());
            list.Add(new RabbitChannel(subscriber, StartQueue(channelName)));
        }

        /// <summary>
        /// 在 Rabbit 服务器中添加一个订阅方法。
        /// </summary>
        /// <param name="channelName">通道名称。</param>
        /// <param name="subscriber">读取数据的方法。</param>
        public void AddSubscriber(string channelName, Action<byte[]> subscriber)
        {
            Guard.ArgumentNull(channelName, nameof(channelName));
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            var list = subscribers.GetOrAdd(channelName, () => new RabbitChannelCollection());
            list.Add(new RabbitChannel(subscriber, StartQueue(channelName)));
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
            Guard.ArgumentNull(subjectType, nameof(subjectType));

            var channelName = ChannelHelper.GetChannelName(subjectType);
            RemoveSubscriber(channelName);
        }

        /// <summary>
        /// 移除指定通道的订阅方法。
        /// </summary>
        /// <param name="channelName">通道名称。</param>
        public void RemoveSubscriber(string channelName)
        {
            Guard.ArgumentNull(channelName, nameof(channelName));

            if (subscribers.TryGetValue(channelName, out RabbitChannelCollection channels))
            {
                channels.ForEach(s => s.Model.QueueDelete(channelName));
            }
        }

        /// <summary>
        /// 序列化对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual T Deserialize<T>(byte[] value)
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
        protected virtual object Deserialize(Type type, byte[] value)
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
        protected virtual byte[] Serialize<T>(T value)
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
                    AutomaticRecoveryEnabled = true,
                }.CreateConnection());
            }

            return connectionLazy.Value;
        }

        /// <summary>
        /// 开启一个队列。
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        private IModel StartQueue(string channelName)
        {
            var channel = GetConnection().CreateModel();
            var queueName = channelName;

            if (string.IsNullOrEmpty(setting.ExchangeType))
            {
                channel.BasicQos(0, 1, false);
                channel.QueueDeclare(channelName, true, false, false, null);
            }
            else
            {
                var exchangeName = GetExchangeName(channelName);
                channel.ExchangeDeclare(exchangeName, setting.ExchangeType);
                queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queueName, exchangeName, channelName);
            }

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, args) =>
                {
                    if (subscribers.TryGetValue(channelName, out RabbitChannelCollection channels))
                    {
                        var found = channels.Find(channel);
                        if (found == null)
                        {
                            return;
                        }

                        var actType = found.Handler.GetType();
                        if (!actType.IsGenericType || actType.GetGenericTypeDefinition() != typeof(Action<>))
                        {
                            return;
                        }

                        var subType = actType.GetGenericArguments()[0];
                        object body = null;

                        try
                        {
                            if (subType == typeof(byte[]))
                            {
                                body = args.Body;
                            }
                            else
                            {
                                body = Deserialize(subType, args.Body);
                            }

                            found.Handler.DynamicInvoke(body);
                            channel.BasicAck(args.DeliveryTag, false);
                        }
                        catch
                        {
                            Thread.Sleep(setting.RequeueDelayTime ?? 1000);
                            channel.BasicNack(args.DeliveryTag, false, true);
                        }
                    }
                };

            //消费消息
            channel.BasicConsume(queueName, false, consumer);

            return channel;
        }

        /// <summary>
        /// 获取交换机的名称。
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        private string GetExchangeName(string channelName)
        {
            return string.Concat(setting.ExchangeType, channelName);
        }

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            this.setting = (RabbitConfigurationSetting)setting;
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return setting;
        }

        private class RabbitChannelCollection : List<RabbitChannel>
        {
            public RabbitChannel Find(IModel model)
            {
                return this.FirstOrDefault(s => s.Model.Equals(model));
            }
        }

        private class RabbitChannel
        {
            public RabbitChannel(Delegate handler, IModel model)
            {
                Handler = handler;
                Model = model;
            }

            public Delegate Handler { get; set; }

            public IModel Model { get; set; }
        }
    }
}
