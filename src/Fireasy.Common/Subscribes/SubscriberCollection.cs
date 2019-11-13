// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System;
using System.Collections.Generic;

namespace Fireasy.Common.Subscribes
{
    /// <summary>
    /// 订阅者集合。
    /// </summary>
    public class SubscriberCollection
    {
        private SafetyDictionary<string, List<SubscribeDelegate>> subscribers = new SafetyDictionary<string, List<SubscribeDelegate>>();

        /// <summary>
        /// 接收订阅的数据。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="data"></param>
        public void Accept(string name, object data)
        {
            if (subscribers.TryGetValue(name, out List<SubscribeDelegate> list) && list != null)
            {
                list.ForEach(s => s.Invoke(data));
            }
        }

        /// <summary>
        /// 添加同步的订阅者。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="subscriber"></param>
        public void AddSyncSubscriber(string name, Delegate subscriber)
        {
            var list = subscribers.GetOrAdd(name, () => new List<SubscribeDelegate>());
            if (list != null)
            {
                list.Add(new SyncSubscribeDelegate(subscriber));
            }
        }

        /// <summary>
        /// 添加异步的订阅者。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="subscriber"></param>
        public void AddAsyncSubscriber(string name, Delegate subscriber)
        {
            var list = subscribers.GetOrAdd(name, () => new List<SubscribeDelegate>());
            if (list != null)
            {
                list.Add(new AsyncSubscribeDelegate(subscriber));
            }
        }

        /// <summary>
        /// 移除订阅者。
        /// </summary>
        /// <param name="name">名称。</param>
        public void Remove(string name)
        {
            subscribers.TryRemove(name, out List<SubscribeDelegate> delegates);
        }
    }
}
