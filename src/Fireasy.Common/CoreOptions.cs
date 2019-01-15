// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Subscribes;
using System;

namespace Fireasy.Common
{
    public class CoreOptions
    {
        /// <summary>
        /// 注册消息订阅器。
        /// </summary>
        /// <param name="name">配置实例名称。</param>
        /// <param name="subscriber">消息订阅器。</param>
        public void AddSubscriber<TSubject>(string name, Action<TSubject> subscriber) where TSubject : class
        {
            SubscribeManagerFactory.CreateManager(name)?.AddSubscriber(subscriber);
        }

        /// <summary>
        /// 配置消息订阅器。
        /// </summary>
        /// <param name="name">配置实例名称。</param>
        /// <returns></returns>
        public SubscribeOptions ConfigSubscribers(string name)
        {
            return new SubscribeOptions(name);
        }
    }

    public class SubscribeOptions
    {
        private string name;

        public SubscribeOptions(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// 注册消息订阅器。
        /// </summary>
        /// <param name="name">配置实例名称。</param>
        /// <param name="subscriber">消息订阅器。</param>
        public void Add<TSubject>(Action<TSubject> subscriber) where TSubject : class
        {
            SubscribeManagerFactory.CreateManager(name)?.AddSubscriber(subscriber);
        }
    }
}
#endif
