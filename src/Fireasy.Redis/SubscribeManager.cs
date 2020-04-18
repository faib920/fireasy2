// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Subscribes.Configuration;
using System.Collections.Generic;
using CSRedis;
#if NETSTANDARD
using Fireasy.Common.Options;
using Microsoft.Extensions.Options;
#endif
using Fireasy.Common.Configuration;
using Fireasy.Common.Subscribes;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Fireasy.Common.Extensions;
using Fireasy.Common;

namespace Fireasy.Redis
{
    /// <summary>
    /// 基于 Redis 的消息订阅管理器。
    /// </summary>
    [ConfigurationSetting(typeof(RedisConfigurationSetting))]
    public class SubscribeManager : RedisComponent, ISubscribeManager
    {
        private readonly SafetyDictionary<string, List<CSRedisClient.SubscribeObject>> channels = new SafetyDictionary<string, List<CSRedisClient.SubscribeObject>>();

        /// <summary>
        /// 初始化 <see cref="SubscribeManager"/> 类的新实例。
        /// </summary>
        public SubscribeManager()
        {
        }

#if NETSTANDARD
        /// <summary>
        /// 初始化 <see cref="SubscribeManager"/> 类的新实例。
        /// </summary>
        /// <param name="options"></param>
        public SubscribeManager(IOptionsMonitor<RedisSubscribeOptions> options)
        {
            var optValue = options.CurrentValue;
            if (!optValue.IsConfigured())
            {
                return;
            }

            RedisConfigurationSetting setting;
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
                    Twemproxy = optValue.Twemproxy,
                    RequeueDelayTime = optValue.RequeueDelayTime
                };

                RedisHelper.ParseHosts(setting, optValue.Hosts);
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
            client.Publish(name, Serialize(subject));
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
            client.Publish(name, Serialize(subject));
        }

        /// <summary>
        /// 异步的，向 Redis 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task PublishAsync<TSubject>(TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            var client = GetConnection(null);
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            try
            {
                await client.PublishAsync(name, Serialize(subject));
            }
            catch (AggregateException exp)
            {
                Tracer.Error($"RedisMQ PublishAsync throw exception:\n{exp.Output()}");
            }
        }

        /// <summary>
        /// 异步的，向 Redis 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task PublishAsync<TSubject>(string name, TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            var client = GetConnection(null);
            try
            {
                await client.PublishAsync(name, Serialize(subject));
            }
            catch (AggregateException exp)
            {
                Tracer.Error($"RedisMQ PublishAsync throw exception:\n{exp.Output()}");
            }

        }

        /// <summary>
        /// 向指定的 Redis 通道发送数据。
        /// </summary>
        /// <param name="name">主题名称。</param>
        /// <param name="data">发送的数据。</param>
        public void Publish(string name, byte[] data)
        {
            var client = GetConnection();
            client.Publish(name, Encoding.UTF8.GetString(data));
        }

        /// <summary>
        /// 异步的，向 Redis 服务器发送消息主题。
        /// </summary>
        /// <param name="name">主题名称。</param>
        /// <param name="data">发送的数据。</param>
        /// <returns></returns>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task PublishAsync(string name, byte[] data, CancellationToken cancellationToken = default)
        {
            var client = GetConnection();
            await client.PublishAsync(name, Encoding.UTF8.GetString(data));
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
            channels.GetOrAdd(name, () => new List<CSRedisClient.SubscribeObject>())
                .Add(client.Subscribe((name, msg =>
                    {
                        Tracer.Debug($"RedisSubscribeManager received the message of '{name}'.");

                        TSubject subject = null;
                        try
                        {
                            subject = Deserialize<TSubject>(msg.Body);
                            subscriber(subject);
                        }
                        catch (Exception exp)
                        {
                            Tracer.Error($"Redis Consume the Topic of '{name}' throw exception:\n{exp.Output()}");

                            if (Setting.RequeueDelayTime != null && subject != null)
                            {
                                Task.Run(() =>
                                    {
                                        Thread.Sleep(Setting.RequeueDelayTime.Value);
                                        Publish(name, subject);
                                    });
                            }
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
            channels.GetOrAdd(name, () => new List<CSRedisClient.SubscribeObject>())
                .Add(client.Subscribe((name, msg =>
                    {
                        Tracer.Debug($"RedisSubscribeManager received the message of '{name}'.");

                        TSubject subject = null;
                        try
                        {
                            subject = Deserialize<TSubject>(msg.Body);
                            subscriber(subject).AsSync();
                        }
                        catch (Exception exp)
                        {
                            Tracer.Error($"Redis Consume the Topic of '{name}' throw exception:\n{exp.Output()}");

                            if (Setting.RequeueDelayTime != null && subject != null)
                            {
                                Task.Run(() =>
                                    {
                                        Thread.Sleep(Setting.RequeueDelayTime.Value);
                                        Publish(name, subject);
                                    });
                            }
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
            channels.GetOrAdd(name, () => new List<CSRedisClient.SubscribeObject>())
                .Add(client.Subscribe((name, msg =>
                    {
                        Tracer.Debug($"RedisSubscribeManager received the message of '{name}'.");

                        object subject = null;
                        try
                        {
                            subject = Deserialize(subjectType, msg.Body);
                            if (subject != null)
                            {
                                subscriber.DynamicInvoke(subject);
                            }
                        }
                        catch (Exception exp)
                        {
                            Tracer.Error($"Redis Consume the Topic of '{name}' throw exception:\n{exp.Output()}");
                            
                            if (Setting.RequeueDelayTime != null)
                            {
                                Task.Run(() =>
                                    {
                                        Thread.Sleep(Setting.RequeueDelayTime.Value);
                                        Publish(name, msg.Body);
                                    });
                            }
                        }
                    }
                )));
        }

        /// <summary>
        /// 在 Redis 服务器中添加一个订阅方法。
        /// </summary>
        /// <param name="name">主题名称。</param>
        /// <param name="subscriber">读取数据的方法。</param>
        public void AddSubscriber(string name, Action<byte[]> subscriber)
        {
            var client = GetConnection();
            channels.GetOrAdd(name, () => new List<CSRedisClient.SubscribeObject>())
                .Add(client.Subscribe((name, msg =>
                    {
                        Tracer.Debug($"RedisSubscribeManager received the message of '{name}'.");

                        var bytes = Encoding.UTF8.GetBytes(msg.Body);
                        try
                        {
                            subscriber.DynamicInvoke(bytes);
                        }
                        catch (Exception exp)
                        {
                            Tracer.Error($"Redis Consume the Topic of '{name}' throw exception:\n{exp.Output()}");
                            
                            if (Setting.RequeueDelayTime != null)
                            {
                                Task.Run(() =>
                                    {
                                        Thread.Sleep(Setting.RequeueDelayTime.Value);
                                        Publish(name, bytes);
                                    });
                            }
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
            var channelName = TopicHelper.GetTopicName(subjectType);
            RemoveSubscriber(channelName);
        }

        /// <summary>
        /// 移除指定通道的订阅方法。
        /// </summary>
        /// <param name="name">主题名称。</param>
        public void RemoveSubscriber(string name)
        {
            var client = GetConnection();
            if (channels.TryRemove(name, out List<CSRedisClient.SubscribeObject> subs) && subs != null)
            {
                subs.ForEach(s => s.Dispose());
            }
        }

        protected override bool Dispose(bool disposing)
        {
            foreach (var kvp in channels)
            {
                kvp.Value.ForEach(s => s.Dispose());
                kvp.Value.Clear();
            }

            channels.Clear();
            return base.Dispose(disposing);
        }
    }
}
