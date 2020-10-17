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
using Fireasy.Common.Subscribes.Persistance;
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
using Fireasy.Common.Ioc;

namespace Fireasy.RabbitMQ
{
    [ConfigurationSetting(typeof(RabbitConfigurationSetting))]
    public class SubscribeManager : DisposableBase, ISubscribeManager, IConfigurationSettingHostService, IServiceProviderAccessor
    {
        private static AliveObject<IConnection> _connectionLazy;
        private static readonly SafetyDictionary<string, RabbitChannelCollection> _subscribers = new SafetyDictionary<string, RabbitChannelCollection>();
        private Func<ISerializer> _serializerFactory;
        private readonly ISubjectPersistance _persistance;
        private readonly ISubscribeNotification _notification;
        private PersistentTimer _timer;

        /// <summary>
        /// 初始化 <see cref="SubscribeManager"/> 类的新实例。
        /// </summary>
        public SubscribeManager()
            : this(ContainerUnity.GetContainer())
        {
        }

        /// <summary>
        /// 初始化 <see cref="SubscribeManager"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        public SubscribeManager(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            _persistance = serviceProvider.TryGetService<ISubjectPersistance>(() => LocalFilePersistance.Default);
            _notification = serviceProvider.TryGetService<ISubscribeNotification>();
        }

#if NETSTANDARD
        /// <summary>
        /// 初始化 <see cref="SubscribeManager"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="options"></param>
        public SubscribeManager(IServiceProvider serviceProvider, IOptionsMonitor<RabbitOptions> options)
            : this(serviceProvider)
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
                    RetryDelayTime = optValue.RetryDelayTime,
                    RetryTimes = optValue.RetryTimes
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
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 获取 RabbitMQ 的相关配置。
        /// </summary>
        protected RabbitConfigurationSetting Setting { get; private set; }

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
            cancellationToken.ThrowIfCancellationRequested();

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
            cancellationToken.ThrowIfCancellationRequested();

            Publish(name, subject);
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

            var list = _subscribers.GetOrAdd(name, () => new RabbitChannelCollection());

            list.Add(new RabbitChannel(new SyncSubscribeDelegate(typeof(TSubject), subscriber), CreateAliveModel(name)));
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

            var list = _subscribers.GetOrAdd(name, () => new RabbitChannelCollection());

            list.Add(new RabbitChannel(new AsyncSubscribeDelegate(typeof(TSubject), subscriber), CreateAliveModel(name)));
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
            var list = _subscribers.GetOrAdd(name, () => new RabbitChannelCollection());

            list.Add(new RabbitChannel(new SyncSubscribeDelegate(subjectType, subscriber), CreateAliveModel(name)));
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

            if (_subscribers.TryGetValue(name, out RabbitChannelCollection channels))
            {
                channels.ForEach(s => s.Model.Value?.QueueDelete(name));
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
            return SingletonLocker.Lock(ref _serializerFactory, this, () =>
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
            _connectionLazy = SingletonLocker.Lock(ref _connectionLazy,
                () => new AliveObject<IConnection>(() => CreateConnectionFactory().CreateConnection(),
                    new AliveCheckPolicy<IConnection>(s => s.IsOpen, TimeSpan.FromSeconds(5))));

            return _connectionLazy.Value;
        }

        /// <summary>
        /// 开启一个队列。
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        private IModel StartQueue(string channelName)
        {
            var connection = GetConnection();
            if (connection == null)
            {
                return null;
            }

            var channel = connection.CreateModel();
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
            consumer.Received += (sender, args) => ConsumeData(sender as CustomEventingBasicConsumer, args);

            //消费消息
            channel.BasicConsume(queueName, false, consumer);

            return channel;
        }

        private AliveObject<IModel> CreateAliveModel(string channelName)
        {
            return new AliveObject<IModel>(() => StartQueue(channelName), new AliveCheckPolicy<IModel>(s => s.IsOpen, TimeSpan.FromSeconds(5)));
        }

        private void ConsumeData(CustomEventingBasicConsumer consumer, BasicDeliverEventArgs args)
        {
            if (!_subscribers.TryGetValue(consumer.ChannelName, out RabbitChannelCollection channels))
            {
                return;
            }

            var found = channels.Find(consumer.Model);
            if (found == null || !consumer.Model.IsOpen)
            {
                return;
            }

            StoredSubject subject = null;
            try
            {
                object body;

                try
                {
                    subject = Deserialize(typeof(StoredSubject), args.Body) as StoredSubject;
                    if (subject == null)
                    {
                        return;
                    }

                    if (found.Handler.DataType == typeof(byte[]))
                    {
                        body = subject.Body;
                    }
                    else
                    {
                        body = Deserialize(found.Handler.DataType, subject.Body);
                    }
                }
                catch (SerializationException)
                {
                    if (found.Handler.DataType == typeof(byte[]))
                    {
                        body = args.Body;
                    }
                    else
                    {
                        body = Deserialize(found.Handler.DataType, args.Body);
                    }
                }

                if (body != null)
                {
                    found.Handler.Invoke(body);
                    consumer.Model.BasicAck(args.DeliveryTag, false);
                }
            }
            catch (Exception exp)
            {
                Tracer.Error($"Throw exception when consume message of '{consumer.ChannelName}':\n{exp.Output()}");

                RetryPublishData(consumer, args, subject, exp);
            }
        }

        private void RetryPublishData(CustomEventingBasicConsumer consumer, BasicDeliverEventArgs args, StoredSubject subject, Exception exception)
        {
            try
            {
                if (!consumer.Model.IsOpen)
                {
                    return;
                }

                consumer.Model.BasicNack(args.DeliveryTag, false, false);

                if (subject != null && Setting.RetryTimes > 0 && subject.AcceptRetries < Setting.RetryTimes)
                {
                    if (_notification != null)
                    {
                        var context = new SubscribeNotificationContext(subject.Name, subject.Body, exception);
                        _notification.OnConsumeError(context);
                        if (!context.CanRetry)
                        {
                            return;
                        }
                    }

                    Task.Run(() =>
                    {
                        Thread.Sleep((int)Setting.RetryDelayTime.TotalMilliseconds);
                        try
                        {
                            subject.AcceptRetries++;
                            PublishSubject(subject);
                        }
                        catch (Exception exp)
                        {
                            PersistSubject(subject, exp);
                        }
                    });
                }
            }
            catch { }
        }

        private void Publish(string name, byte[] data)
        {
            var pdata = new StoredSubject(name, data);

            try
            {
                PublishSubject(pdata);
            }
            catch (Exception exp)
            {
                if (Setting.RetryTimes > 0)
                {
                    PersistSubject(pdata, exp);
                }
                else
                {
                    throw exp;
                }
            }
        }

        /// <summary>
        /// 发送数据。
        /// </summary>
        /// <param name="subject"></param>
        private void PublishSubject(StoredSubject subject)
        {
            var connection = GetConnection();
            using (var channel = connection.CreateModel())
            {
                var bytes = Serialize(subject);

                if (string.IsNullOrEmpty(Setting.ExchangeType))
                {
                    channel.QueueDeclare(subject.Name, true, false, false, null);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.DeliveryMode = 2;

                    channel.BasicPublish(string.Empty, subject.Name, properties, bytes);
                }
                else
                {
                    var exchangeName = GetExchangeName(subject.Name);
                    channel.ExchangeDeclare(exchangeName, Setting.ExchangeType);
                    channel.BasicPublish(exchangeName, subject.Name, null, bytes);
                }
            }
        }

        private void PersistSubject(StoredSubject subject, Exception exception)
        {
            if (_persistance != null && subject.PublishRetries == 0)
            {
                if (_notification != null)
                {
                    var context = new SubscribeNotificationContext(subject.Name, subject.Body, exception);
                    _notification.OnPublishError(context);
                    if (!context.CanRetry)
                    {
                        return;
                    }
                }

                if (_persistance.SaveSubject("rabbit", subject))
                {
                    Tracer.Debug($"{_persistance} was persisted of '{subject.Name}'.");
                }
            }
        }

        /// <summary>
        /// 开启重新发送的定时器。
        /// </summary>
        private void StartPersistentTimer()
        {
            if (_persistance == null || Setting.RetryTimes == null || Setting.RetryTimes == 0)
            {
                return;
            }

            SingletonLocker.Lock(ref _timer, () =>
            {
                return new PersistentTimer(Setting.RetryDelayTime, () =>
                {
                    _persistance.ReadSubjects("rabbit", subject =>
                    {
                        //如果超出重试次数
                        if (Setting.RetryTimes != null && subject.PublishRetries > Setting.RetryTimes)
                        {
                            return true;
                        }

                        try
                        {
                            Tracer.Debug($"Republish message of '{subject.Name}'.");
                            PublishSubject(subject);
                            return true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    });
                });
            });
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
            StartPersistentTimer();
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return Setting;
        }

        protected override bool Dispose(bool disposing)
        {
            if (_connectionLazy != null && _connectionLazy.IsValueCreated)
            {
                _connectionLazy.Value.Dispose();
            }

            _subscribers?.ForEach(s => s.Value.Dispose());

            return base.Dispose(disposing);
        }

        private class RabbitChannelCollection : DisposableBase
        {
            private readonly List<RabbitChannel> channels = new List<RabbitChannel>();

            public RabbitChannel Find(IModel model)
            {
                return channels.FirstOrDefault(s => model.Equals(s.Model.Value));
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

        private class RabbitChannel : DisposableBase
        {
            public RabbitChannel(SubscribeDelegate handler, AliveObject<IModel> model)
            {
                Handler = handler;
                Model = model;
                _ = model.Value;
            }

            public SubscribeDelegate Handler { get; private set; }

            public AliveObject<IModel> Model { get; private set; }

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
