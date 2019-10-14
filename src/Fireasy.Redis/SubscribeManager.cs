// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.ComponentModel;
using System.Collections.Generic;
using CSRedis;
#endif
using Fireasy.Common.Configuration;
using Fireasy.Common.Subscribes;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Fireasy.Redis
{
    /// <summary>
    /// 基于 Redis 的消息订阅管理器。
    /// </summary>
    [ConfigurationSetting(typeof(RedisConfigurationSetting))]
    public class SubscribeManager : RedisComponent, ISubscribeManager
    {
#if NETSTANDARD
        private SafetyDictionary<string, List<CSRedisClient.SubscribeObject>> channels = new SafetyDictionary<string, List<CSRedisClient.SubscribeObject>>();
#endif

        /// <summary>
        /// 向 Redis 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        public void Publish<TSubject>(TSubject subject) where TSubject : class
        {
#if NETSTANDARD
            var client = GetConnection();
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            client.Publish(name, Serialize(subject));
#else
            var data = Encoding.UTF8.GetBytes(Serialize(subject));
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            Publish(name, data);
#endif
        }

        /// <summary>
        /// 向 Redis 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="name">主题名称。</param>
        /// <param name="subject">主题内容。</param>
        public void Publish<TSubject>(string name, TSubject subject) where TSubject : class
        {
#if NETSTANDARD
            var client = GetConnection();
            client.Publish(name, Serialize(subject));
#else
            var data = Encoding.UTF8.GetBytes(Serialize(subject));
            Publish(name, data);
#endif
        }

        /// <summary>
        /// 异步的，向 Redis 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task PublishAsync<TSubject>(TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
#if NETSTANDARD
            var client = GetConnection();
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            await client.PublishAsync(name, Serialize(subject));
#else
            var data = Encoding.UTF8.GetBytes(Serialize(subject));
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            Publish(name, data);
#endif
        }

        /// <summary>
        /// 异步的，向 Redis 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task PublishAsync<TSubject>(string name, TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
#if NETSTANDARD
            var client = GetConnection();
            await client.PublishAsync(name, Serialize(subject));
#else
            var data = Encoding.UTF8.GetBytes(Serialize(subject));
            Publish(name, data);
#endif
        }

        /// <summary>
        /// 向指定的 Redis 通道发送数据。
        /// </summary>
        /// <param name="name">主题名称。</param>
        /// <param name="data">发送的数据。</param>
        public void Publish(string name, byte[] data)
        {
            var client = GetConnection();
#if NETSTANDARD
            client.Publish(name, Encoding.UTF8.GetString(data));
#else
            client.GetSubscriber().Publish(name, data);
#endif        
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
#if NETSTANDARD
            await client.PublishAsync(name, Encoding.UTF8.GetString(data));
#else
            client.GetSubscriber().Publish(name, data);
#endif        
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
#if NETSTANDARD
            channels.GetOrAdd(name, () => new List<CSRedisClient.SubscribeObject>())
                .Add(client.Subscribe((name, msg =>
                {
                    var subject = Deserialize<TSubject>(msg.Body);
                    subscriber(subject);
                }
            )));
#else
            client.GetSubscriber().Subscribe(name, (channel, value) =>
                {
                    var subject = Deserialize<TSubject>(Encoding.UTF8.GetString(value));
                    subscriber(subject);
                });
#endif
        }

        /// <summary>
        /// 在 Redis 服务器中添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="name">主题名称。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber<TSubject>(string name, Action<TSubject> subscriber) where TSubject : class
        {
            var client = GetConnection();
#if NETSTANDARD
            channels.GetOrAdd(name, () => new List<CSRedisClient.SubscribeObject>())
                .Add(client.Subscribe((name, msg =>
                {
                    var subject = Deserialize<TSubject>(msg.Body);
                    subscriber(subject);
                }
            )));
#else
            client.GetSubscriber().Subscribe(name, (channel, value) =>
                {
                    var subject = Deserialize<TSubject>(Encoding.UTF8.GetString(value));
                    subscriber(subject);
                });
#endif
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
#if NETSTANDARD
            channels.GetOrAdd(name, () => new List<CSRedisClient.SubscribeObject>())
                .Add(client.Subscribe((name, msg =>
                {
                    var subject = Deserialize(subjectType, msg.Body);
                    if (subject != null)
                    {
                        subscriber.DynamicInvoke(subject);
                    }
                }
            )));
#else
            client.GetSubscriber().Subscribe(name, (channel, value) =>
                {
                    var subject = Deserialize(subjectType, Encoding.UTF8.GetString(value));
                    if (subject != null)
                    {
                        subscriber.DynamicInvoke(subject);
                    }
                });
#endif
        }

        /// <summary>
        /// 在 Redis 服务器中添加一个订阅方法。
        /// </summary>
        /// <param name="name">主题名称。</param>
        /// <param name="subscriber">读取数据的方法。</param>
        public void AddSubscriber(string name, Action<byte[]> subscriber)
        {
            var client = GetConnection();
#if NETSTANDARD
            channels.GetOrAdd(name, () => new List<CSRedisClient.SubscribeObject>())
                .Add(client.Subscribe((name, msg =>
                {
                    subscriber.DynamicInvoke(Encoding.UTF8.GetBytes(msg.Body));
                }
            )));
#else
            client.GetSubscriber().Subscribe(name, (c, value) =>
                {
                    subscriber.DynamicInvoke((byte[])value);
                });
#endif        
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
            var client = GetConnection();
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
#if NETSTANDARD
            if (channels.TryRemove(name, out List<CSRedisClient.SubscribeObject> subs) && subs != null)
            {
                subs.ForEach(s => s.Dispose());
            }
#else
            client.GetSubscriber().Unsubscribe(name);
#endif
        }
    }
}
