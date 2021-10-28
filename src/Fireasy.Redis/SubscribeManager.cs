// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System.Collections.Generic;
using CSRedis;
#if NETSTANDARD
using Fireasy.Common.Options;
using Microsoft.Extensions.Options;
using Fireasy.Common.Subscribes.Configuration;
using System.Linq;
#endif
using Fireasy.Common.Configuration;
using Fireasy.Common.Subscribes;
using Fireasy.Common.Serialization;
using System;
using System.Threading.Tasks;
using System.Threading;
using Fireasy.Common.Extensions;
using Fireasy.Common;
using Fireasy.Common.Subscribes.Persistance;
using Fireasy.Common.Ioc;
using Fireasy.Common.Threading;

namespace Fireasy.Redis
{
    /// <summary>
    /// 基于 Redis 的消息订阅管理器。
    /// </summary>
    [ConfigurationSetting(typeof(RedisConfigurationSetting))]
    public class SubscribeManager : RedisComponent, ISubscribeManager
    {
        private readonly SafetyDictionary<string, List<CSRedisClient.SubscribeObject>> _channels = new SafetyDictionary<string, List<CSRedisClient.SubscribeObject>>();
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
            : base(serviceProvider)
        {
            _persistance = serviceProvider.TryGetService<ISubjectPersistance>(() => LocalFilePersistance.Default);
            _notification = serviceProvider.TryGetService<ISubscribeNotification>();
        }

#if NETSTANDARD
        /// <summary>
        /// 初始化 <see cref="SubscribeManager"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="options"></param>
        public SubscribeManager(IServiceProvider serviceProvider, IOptionsMonitor<RedisSubscribeOptions> options)
            : this(serviceProvider)
        {
            RedisConfigurationSetting setting = null;
            var optValue = options.CurrentValue;
            if (!optValue.IsConfigured())
            {
                var section = ConfigurationUnity.GetSection<SubscribeConfigurationSection>();
                var matchSetting = section.Settings.FirstOrDefault(s => s.Value.SubscriberType == typeof(SubscribeManager)).Value;
                if (matchSetting != null && section.GetSetting(matchSetting.Name) is ExtendConfigurationSetting extSetting)
                {
                    setting = (RedisConfigurationSetting)extSetting.Extend;
                }
                else
                {
                    throw new InvalidOperationException($"未发现与 {typeof(SubscribeManager).FullName} 相匹配的配置。");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(optValue.ConfigName))
                {
                    var section = ConfigurationUnity.GetSection<SubscribeConfigurationSection>();
                    if (section != null && section.GetSetting(optValue.ConfigName) is ExtendConfigurationSetting extSetting)
                    {
                        setting = (RedisConfigurationSetting)extSetting.Extend;
                    }
                    else
                    {
                        throw new InvalidOperationException($"无效的配置节: {optValue.ConfigName}。");
                    }
                }
                else
                {
                    setting = new RedisConfigurationSetting
                    {
                        Password = optValue.Password,
                        ConnectionString = optValue.ConnectionString,
                        DefaultDb = optValue.DefaultDb,
                        ConnectTimeout = optValue.ConnectTimeout,
                        LockTimeout = optValue.LockTimeout,
                        SyncTimeout = optValue.SyncTimeout,
                        WriteBuffer = optValue.WriteBuffer,
                        PoolSize = optValue.PoolSize,
                        SerializerType = optValue.SerializerType,
                        Ssl = optValue.Ssl,
                        Prefix = optValue.Prefix,
                        RetryDelayTime = optValue.RetryDelayTime,
                        RetryTimes = optValue.RetryTimes,
                        Preheat = optValue.Preheat
                    };

                    RedisHelper.ParseHosts(setting, optValue.Hosts, optValue.Sentinels);
                }
            }

            if (setting != null)
            {
                (this as IConfigurationSettingHostService).Attach(setting);
            }

            optValue.Initializer?.Invoke(this);
        }
#endif

        /// <summary>
        /// 向 Redis 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        public void Publish<TSubject>(TSubject subject) where TSubject : class
        {
            var client = GetConnection(null);
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            var body = SerializeToBytes(subject);

            Publish(client, name, body);
        }

        /// <summary>
        /// 向 Redis 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="name">主题名称。</param>
        /// <param name="subject">主题内容。</param>
        public void Publish<TSubject>(string name, TSubject subject) where TSubject : class
        {
            var client = GetConnection(null);
            var body = SerializeToBytes(subject);

            Publish(client, name, body);
        }

        /// <summary>
        /// 异步的，向 Redis 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task PublishAsync<TSubject>(TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            cancellationToken.ThrowIfCancellationRequested();

            var client = GetConnection(null);
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            var body = SerializeToBytes(subject);
            await PublishAsync(client, name, body);
        }

        /// <summary>
        /// 异步的，向 Redis 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task PublishAsync<TSubject>(string name, TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            cancellationToken.ThrowIfCancellationRequested();

            var client = GetConnection(null);
            var body = SerializeToBytes(subject);
            await PublishAsync(client, name, body);
        }

        /// <summary>
        /// 在 Redis 服务器中添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber<TSubject>(Action<TSubject> subscriber) where TSubject : class
        {
            var client = GetConnection();
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            AddSubscriber<TSubject>(name, subscriber);
        }

        /// <summary>
        /// 在 Redis 服务器中添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddAsyncSubscriber<TSubject>(Func<TSubject, Task> subscriber) where TSubject : class
        {
            var client = GetConnection();
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            AddAsyncSubscriber<TSubject>(name, subscriber);
        }

        /// <summary>
        /// 在 Redis 服务器中添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="name">主题名称。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber<TSubject>(string name, Action<TSubject> subscriber) where TSubject : class
        {
            var client = GetConnection(null);
            name = TopicHelper.GetTopicName(ServiceProvider, name);
            _channels.GetOrAdd(name, () => new List<CSRedisClient.SubscribeObject>())
                .Add(client.Subscribe((name, msg =>
                    {
                        StoredSubject subject = null;
                        try
                        {
                            try
                            {
                                subject = Deserialize<StoredSubject>(msg.Body);
                                subscriber(Deserialize<TSubject>(subject.Body));
                            }
                            catch (SerializationException)
                            {
                                subscriber(Deserialize<TSubject>(msg.Body));
                            }
                        }
                        catch (Exception exp)
                        {
                            Tracer.Error($"Throw exception when consume message of '{name}':\n{exp.Output()}");

                            RetryPublishData(subject, exp);
                        }
                    }
            )));
        }


        /// <summary>
        /// 在 Redis 服务器中添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="name">主题名称。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddAsyncSubscriber<TSubject>(string name, Func<TSubject, Task> subscriber) where TSubject : class
        {
            var client = GetConnection(null);
            name = TopicHelper.GetTopicName(ServiceProvider, name);
            _channels.GetOrAdd(name, () => new List<CSRedisClient.SubscribeObject>())
                .Add(client.Subscribe((name, msg =>
                    {
                        StoredSubject subject = null;
                        try
                        {
                            try
                            {
                                subject = Deserialize<StoredSubject>(msg.Body);
                                subscriber(Deserialize<TSubject>(subject.Body)).AsSync();
                            }
                            catch (SerializationException)
                            {
                                subscriber(Deserialize<TSubject>(msg.Body)).AsSync();
                            }
                        }
                        catch (Exception exp)
                        {
                            Tracer.Error($"Throw exception when consume message of '{name}':\n{exp.Output()}");

                            RetryPublishData(subject, exp);
                        }
                    }
            )));
        }

        /// <summary>
        /// 在 Redis 服务器中添加一个订阅方法。
        /// </summary>
        /// <param name="subjectType">主题的类型。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber(Type subjectType, Delegate subscriber)
        {
            var client = GetConnection();
            var name = TopicHelper.GetTopicName(subjectType);
            name = TopicHelper.GetTopicName(ServiceProvider, name);
            _channels.GetOrAdd(name, () => new List<CSRedisClient.SubscribeObject>())
                .Add(client.Subscribe((name, msg =>
                    {
                        StoredSubject subject = null;
                        try
                        {
                            try
                            {
                                subject = Deserialize<StoredSubject>(msg.Body);
                                subscriber.DynamicInvoke(Deserialize(subjectType, subject.Body));
                            }
                            catch (SerializationException)
                            {
                                subscriber.DynamicInvoke(Deserialize(subjectType, msg.Body));
                            }
                        }
                        catch (Exception exp)
                        {
                            Tracer.Error($"Throw exception when consume message of '{name}':\n{exp.Output()}");

                            RetryPublishData(subject, exp);
                        }
                    }
            )));
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
            var name = TopicHelper.GetTopicName(subjectType);
            RemoveSubscriber(name);
        }

        /// <summary>
        /// 移除指定通道的订阅方法。
        /// </summary>
        /// <param name="name">主题名称。</param>
        public void RemoveSubscriber(string name)
        {
            name = TopicHelper.GetTopicName(ServiceProvider, name);
            var client = GetConnection();
            if (_channels.TryRemove(name, out List<CSRedisClient.SubscribeObject> subs) && subs != null)
            {
                subs.ForEach(s => s.Dispose());
            }
        }

        protected override void OnInitialize()
        {
            StartPersistentTimer();
            base.OnInitialize();
        }

        private void RetryPublishData(StoredSubject subject, Exception exception)
        {
            try
            {
                if (subject != null && Setting.RetryTimes > 0 && subject.AcceptRetries < Setting.RetryTimes)
                {
                    var hasNext = subject.AcceptRetries < Setting.RetryTimes - 1;
                    if (_notification != null)
                    {
                        using var scope = ServiceProvider.TryCreateScope();
                        var context = new SubscribeNotificationContext(scope.ServiceProvider, subject.Name, subject.Body, exception, GetSerializer(), hasNext);
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
                            var client = GetConnection(null);
                            subject.AcceptRetries++;
                            PublishSubject(client, subject);
                        }
                        catch (Exception exp)
                        {
                            PersistSubject(subject, exp, hasNext);
                        }
                    });
                }
            }
            catch { }
        }

        private void Publish(CSRedisClient client, string name, byte[] data)
        {
            name = TopicHelper.GetTopicName(ServiceProvider, name);
            var pdata = new StoredSubject(name, data);

            try
            {
                PublishSubject(client, pdata);
            }
            catch (Exception exp)
            {
                if (Setting.RetryTimes > 0)
                {
                    PersistSubject(pdata, exp, true);
                }
                else
                {
                    throw exp;
                }
            }
        }

        private async Task PublishAsync(CSRedisClient client, string name, byte[] data)
        {
            name = TopicHelper.GetTopicName(ServiceProvider, name);
            var pdata = new StoredSubject(name, data);

            try
            {
                await PublishSubjectAsync(client, pdata);
            }
            catch (Exception exp)
            {
                if (Setting.RetryTimes > 0)
                {
                    PersistSubject(pdata, exp, Setting.RetryTimes > 1);
                }
                else
                {
                    throw exp;
                }
            }
        }

        private void PublishSubject(CSRedisClient client, StoredSubject subject)
        {
            client.Publish(subject.Name, Serialize(subject));
        }

        private async Task PublishSubjectAsync(CSRedisClient client, StoredSubject subject)
        {
            await client.PublishAsync(subject.Name, Serialize(subject)).ConfigureAwait(false);
        }

        private void PersistSubject(StoredSubject subject, Exception exception, bool hasNext)
        {
            if (_persistance != null && subject.PublishRetries == 0)
            {
                if (_notification != null)
                {
                    using var scope = ServiceProvider.TryCreateScope();
                    var context = new SubscribeNotificationContext(scope.ServiceProvider, subject.Name, subject.Body, exception, GetSerializer(), hasNext);
                    _notification.OnPublishError(context);
                    if (!context.CanRetry)
                    {
                        return;
                    }
                }

                if (_persistance.SaveSubject("redis", subject))
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
                    _persistance.ReadSubjects("redis", subject =>
                    {
                        //如果超出重试次数
                        if (Setting.RetryTimes != null && subject.PublishRetries > Setting.RetryTimes)
                        {
                            return SubjectRetryStatus.OutOfTimes;
                        }

                        try
                        {
                            Tracer.Debug($"Republish message of '{subject.Name}'.");
                            var client = GetConnection(null);
                            PublishSubject(client, subject);
                            return SubjectRetryStatus.Success;
                        }
                        catch (Exception exception)
                        {
                            var hasNext = subject.PublishRetries < Setting.RetryTimes;
                            if (_notification != null)
                            {
                                using var scope = ServiceProvider.TryCreateScope();
                                var context = new SubscribeNotificationContext(scope.ServiceProvider, subject.Name, subject.Body, exception, GetSerializer(), hasNext);
                                _notification.OnPublishError(context);
                                if (!context.CanRetry)
                                {
                                    return SubjectRetryStatus.Failed;
                                }
                            }

                            return SubjectRetryStatus.Failed;
                        }
                    });
                });
            });
        }

        protected override bool Dispose(bool disposing)
        {
            foreach (var kvp in _channels)
            {
                kvp.Value.ForEach(s => s.Dispose());
                kvp.Value.Clear();
            }

            _channels.Clear();

            return base.Dispose(disposing);
        }
    }
}
