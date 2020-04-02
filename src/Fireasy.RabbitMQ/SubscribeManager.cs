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
using Fireasy.Common.Serialization;
using Fireasy.Common.Subscribes;
#if NETSTANDARD
using Fireasy.Common.Options;
using Fireasy.Common.Subscribes.Configuration;
using Microsoft.Extensions.Options;
#endif
using Fireasy.Common.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.RabbitMQ
{
    [ConfigurationSetting(typeof(RabbitConfigurationSetting))]
    public class SubscribeManager : DisposeableBase, ISubscribeManager, IConfigurationSettingHostService, IServiceProviderAccessor
    {
        private static Lazy<IConnection> connectionLazy;
        private static readonly SafetyDictionary<string, RabbitChannelCollection> subscribers = new SafetyDictionary<string, RabbitChannelCollection>();
        private Func<ISerializer> serializerFactory;

        /// <summary>
        /// 初始化 <see cref="SubscribeManager"/> 类的新实例。
        /// </summary>
        public SubscribeManager()
        {
        }

        /// <summary>
        /// 初始化 <see cref="SubscribeManager"/> 类的新实例。
        /// </summary>
        /// <param name="ServiceProvider"></param>
        public SubscribeManager(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

#if NETSTANDARD
        /// <summary>
        /// 初始化 <see cref="SubscribeManager"/> 类的新实例。
        /// </summary>
        /// <param name="options"></param>
        public SubscribeManager(IServiceProvider serviceProvider, IOptionsMonitor<RabbitOptions> options)
            : this (serviceProvider)
        {
            var optValue = options.CurrentValue;
            if (!optValue.IsConfigured())
            {
                return;
            }

            RabbitConfigurationSetting setting = null;
            if (!string.IsNullOrEmpty(optValue.ConfigName))
            {
                var section = ConfigurationUnity.GetSection<SubscribeConfigurationSection>();
                if (section != null && section.GetSetting(optValue.ConfigName) is ExtendConfigurationSetting extSetting)
                {
                    setting = (RabbitConfigurationSetting)extSetting.Extend;
                }
            }
            else
            {
                setting = new RabbitConfigurationSetting
                {
                    Server = optValue.Server,
                    Port = optValue.Port ?? -1,
                    Password = optValue.Password,
                    UserName = optValue.UserName,
                    ExchangeType = optValue.ExchangeType,
                    VirtualHost = optValue.VirtualHost,
                    RequeueDelayTime = optValue.RequeueDelayTime
                };
            }

            if (setting != null)
            {
                (this as IConfigurationSettingHostService).Attach(setting);
            }

            optValue.Initializer?.Invoke(this);
        }
#endif

        /// <summary>
        /// 获取 RabbitMQ 的相关配置。
        /// </summary>
        protected RabbitConfigurationSetting Setting { get; private set; }

        /// <summary>
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 向 Rabbit 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        public void Publish<TSubject>(TSubject subject) where TSubject : class
        {
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            var data = Serialize(subject);

            Publish(name, data);
        }

        /// <summary>
        /// 向 Rabbit 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="name">主题名称。</param>
        /// <param name="subject">主题内容。</param>
        public void Publish<TSubject>(string name, TSubject subject) where TSubject : class
        {
            var data = Serialize(subject);

            Publish(name, data);
        }

        /// <summary>
        /// 异步的，向 Rabbit 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task PublishAsync<TSubject>(TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            Publish(subject);
        }

        /// <summary>
        /// 异步的，向 Rabbit 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="name">主题名称。</param>
        /// <param name="subject">主题内容。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task PublishAsync<TSubject>(string name, TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            Publish(name, subject);
        }

        /// <summary>
        /// 向指定的 Rabbit 通道发送数据。
        /// </summary>
        /// <param name="name">主题名称。</param>
        /// <param name="data">发送的数据。</param>
        public void Publish(string name, byte[] data)
        {
            using (var channel = GetConnection().CreateModel())
            {
                if (string.IsNullOrEmpty(Setting.ExchangeType))
                {
                    channel.QueueDeclare(name, true, false, false, null);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.DeliveryMode = 2;

                    channel.BasicPublish(string.Empty, name, properties, data);
                }
                else
                {
                    var exchangeName = GetExchangeName(name);
                    channel.ExchangeDeclare(exchangeName, Setting.ExchangeType);
                    channel.BasicPublish(exchangeName, name, null, data);
                }
            }
        }

        /// <summary>
        /// 异步的，向 Rabbit 服务器发送消息主题。
        /// </summary>
        /// <param name="name">主题名称。</param>
        /// <param name="data">发送的数据。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task PublishAsync(string name, byte[] data, CancellationToken cancellationToken = default)
        {
            Publish(name, data);
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
        /// 在 Rabbit 服务器中添加一个异步的订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddAsyncSubscriber<TSubject>(Func<TSubject, Task> subscriber) where TSubject : class
        {
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            var name = TopicHelper.GetTopicName(typeof(TSubject));
            AddAsyncSubscriber<TSubject>(name, subscriber);
        }

        /// <summary>
        /// 在 Rabbit 服务器中添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="name">主题名称。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber<TSubject>(string name, Action<TSubject> subscriber) where TSubject : class
        {
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            var list = subscribers.GetOrAdd(name, () => new RabbitChannelCollection());
            list.Add(new RabbitChannel(new SyncSubscribeDelegate(typeof(TSubject), subscriber), StartQueue(name)));
        }

        /// <summary>
        /// 在 Rabbit 服务器中添加一个异步的订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="name">主题名称。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddAsyncSubscriber<TSubject>(string name, Func<TSubject, Task> subscriber) where TSubject : class
        {
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            var list = subscribers.GetOrAdd(name, () => new RabbitChannelCollection());
            list.Add(new RabbitChannel(new AsyncSubscribeDelegate(typeof(TSubject), subscriber), StartQueue(name)));
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

            var name = TopicHelper.GetTopicName(subjectType);
            var list = subscribers.GetOrAdd(name, () => new RabbitChannelCollection());
            list.Add(new RabbitChannel(new SyncSubscribeDelegate(subjectType, subscriber), StartQueue(name)));
        }

        /// <summary>
        /// 在 Rabbit 服务器中添加一个订阅方法。
        /// </summary>
        /// <param name="name">主题名称。</param>
        /// <param name="subscriber">读取数据的方法。</param>
        public void AddSubscriber(string name, Action<byte[]> subscriber)
        {
            Guard.ArgumentNull(name, nameof(name));
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            var list = subscribers.GetOrAdd(name, () => new RabbitChannelCollection());
            list.Add(new RabbitChannel(new SyncSubscribeDelegate(null, subscriber), StartQueue(name)));
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

            var channelName = TopicHelper.GetTopicName(subjectType);
            RemoveSubscriber(channelName);
        }

        /// <summary>
        /// 移除指定通道的订阅方法。
        /// </summary>
        /// <param name="name">主题名称。</param>
        public void RemoveSubscriber(string name)
        {
            Guard.ArgumentNull(name, nameof(name));

            if (subscribers.TryGetValue(name, out RabbitChannelCollection channels))
            {
                channels.ForEach(s => s.Model.QueueDelete(name));
            }
        }

        /// <summary>
        /// 序列化对象。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual object Deserialize(Type type, byte[] value)
        {
            var serializer = GetSerializer();

            return serializer.Deserialize(value, type);
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual byte[] Serialize<T>(T value)
        {
            var serializer = GetSerializer();
            return serializer.Serialize(value);
        }

        /// <summary>
        /// 创建 <see cref="ConnectionFactory"/> 实例。
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        protected virtual ConnectionFactory CreateConnectionFactory()
        {
            return new ConnectionFactory
            {
                UserName = Setting.UserName,
                Password = Setting.Password,
                Endpoint = new AmqpTcpEndpoint(new Uri(Setting.Server)),
                Port = Setting.Port,
                RequestedHeartbeat = 12,
                AutomaticRecoveryEnabled = true,
                VirtualHost = string.IsNullOrEmpty(Setting.VirtualHost) ? "/" : Setting.VirtualHost
            };
        }

        private ISerializer GetSerializer()
        {
            return SingletonLocker.Lock(ref serializerFactory, this, () =>
            {
                return new Func<ISerializer>(() =>
                {
                    ISerializer _serializer = null;
                    if (Setting.SerializerType != null)
                    {
                        _serializer = Setting.SerializerType.New<ISerializer>();
                    }

                    if (_serializer == null)
                    {
                        _serializer = ServiceProvider.TryGetService(() => SerializerFactory.CreateSerializer());
                    }

                    if (_serializer == null)
                    {
                        var option = new JsonSerializeOption();
                        option.Converters.Add(new FullDateTimeJsonConverter());
                        _serializer = new JsonSerializer(option);
                    }

                    return _serializer;
                });
            })();
        }

        private IConnection GetConnection()
        {
            connectionLazy = SingletonLocker.Lock(ref connectionLazy, () =>
                {
                    return new Lazy<IConnection>(() => CreateConnectionFactory().CreateConnection());
                });

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

            if (string.IsNullOrEmpty(Setting.ExchangeType))
            {
                channel.BasicQos(0, 1, false);
                channel.QueueDeclare(channelName, true, false, false, null);
            }
            else
            {
                var exchangeName = GetExchangeName(channelName);
                channel.ExchangeDeclare(exchangeName, Setting.ExchangeType);
                queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queueName, exchangeName, channelName);
            }

            var consumer = new CustomEventingBasicConsumer(channel, channelName);
            consumer.Received += (sender, args) =>
                {
                    Tracer.Debug($"RabbitMQSubscribeManager accept message of '{channelName}'.");

                    var _consumer = sender as CustomEventingBasicConsumer;
                    if (subscribers.TryGetValue(_consumer.ChannelName, out RabbitChannelCollection channels))
                    {
                        var found = channels.Find(_consumer.Model);
                        if (found == null)
                        {
                            return;
                        }

                        object body = null;

                        try
                        {
                            if (found.Handler.DataType == typeof(byte[]))
                            {
                                body = args.Body;
                            }
                            else
                            {
                                body = Deserialize(found.Handler.DataType, args.Body);
                            }

                            found.Handler.Invoke(body);
                            _consumer.Model.BasicAck(args.DeliveryTag, false);
                        }
                        catch (Exception exp)
                        {
                            Tracer.Error($"RabbitMQSubscribeManager Consume the Topic of '{channelName}':\n{exp.Output()}");

                            _consumer.Model.BasicNack(args.DeliveryTag, false, false);

                            if (Setting.RequeueDelayTime != null)
                            {
                                Task.Run(() =>
                                    {
                                        Thread.SpinWait(Setting.RequeueDelayTime.Value);
                                        Publish(_consumer.ChannelName, args.Body);
                                    });
                            }
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
            return string.Concat(Setting.ExchangeType, channelName);
        }

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            Setting = (RabbitConfigurationSetting)setting;
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return Setting;
        }

        protected override bool Dispose(bool disposing)
        {
            if (connectionLazy != null && connectionLazy.IsValueCreated)
            {
                connectionLazy.Value.Dispose();
            }

            subscribers?.ForEach(s => s.Value.Dispose());

            return base.Dispose(disposing);
        }

        private class RabbitChannelCollection : DisposeableBase
        {
            private readonly List<RabbitChannel> channels = new List<RabbitChannel>();

            public RabbitChannel Find(IModel model)
            {
                return channels.FirstOrDefault(s => s.Model.Equals(model));
            }

            public void Add(RabbitChannel channel)
            {
                channels.Add(channel);
            }

            public void ForEach(Action<RabbitChannel> action)
            {
                channels.ForEach(action);
            }

            protected override bool Dispose(bool disposing)
            {
                channels.ForEach(s => s.Dispose());
                channels.Clear();

                return base.Dispose(disposing);
            }
        }

        private class RabbitChannel : DisposeableBase
        {
            public RabbitChannel(SubscribeDelegate handler, IModel model)
            {
                Handler = handler;
                Model = model;
            }

            public SubscribeDelegate Handler { get; private set; }

            public IModel Model { get; private set; }

            protected override bool Dispose(bool disposing)
            {
                Model?.Dispose();
                return base.Dispose(disposing);
            }
        }

        private struct RequeueData
        {
            public string Name { get; set; }

            public byte[] Data { get; set; }
        }

        private class CustomEventingBasicConsumer : EventingBasicConsumer
        {
            public CustomEventingBasicConsumer(IModel model, string channelName)
                : base(model)
            {
                ChannelName = channelName;
            }

            public string ChannelName { get; private set; }
        }
    }
}
