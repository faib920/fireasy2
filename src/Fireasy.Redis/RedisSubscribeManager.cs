// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Subscribes;
using System;

namespace Fireasy.Redis
{
    /// <summary>
    /// /// 基于 Redis 的消息订阅管理器。
    /// </summary>
    [ConfigurationSetting(typeof(RedisConfigurationSetting))]
    public class RedisSubscribeManager : RedisComponent, ISubscribeManager
    {
        /// <summary>
        /// 在 Redis 服务器中添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscribe">读取主题的方法。</param>
        public void AddSubscriber<TSubject>(Action<TSubject> subscriber) where TSubject : ISubject
        {
            var client = GetConnection();
            client.GetSubscriber().Subscribe(typeof(TSubject).FullName, (channel, value) =>
            {
                var subject = Deserialize<TSubject>(value.ToString());
                subscriber(subject);
            });
        }

        /// <summary>
        /// 在 Redis 服务器中添加一个订阅方法。
        /// </summary>
        /// <param name="subjectType">主题的类型。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber(Type subjectType, Delegate subscriber)
        {
            var client = GetConnection();
            client.GetSubscriber().Subscribe(subjectType.FullName, (channel, value) =>
            {
                var subject = Deserialize(subjectType, value.ToString());
                if (subject != null)
                {
                    subscriber.DynamicInvoke(subject);
                }
            });
        }

        /// <summary>
        /// 向 Redis 服务器发送消息主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        public void Publish<TSubject>(TSubject subject) where TSubject : ISubject
        {
            var client = GetConnection();
            client.GetSubscriber().Publish(typeof(TSubject).FullName, Serialize(subject));
        }
    }
}
