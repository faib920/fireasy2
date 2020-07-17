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
        private readonly SafetyDictionary<string, List<SubscribeDelegate>> _subscribers = new SafetyDictionary<string, List<SubscribeDelegate>>();

        /// <summary>
        /// 接收订阅的数据。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="data"></param>
        public void Accept(string name, object data)
        {
            if (_subscribers.TryGetValue(name, out List<SubscribeDelegate> list) && list != null)
            {
                list.ForEach(s => s.Invoke(data));
            }
        }

        /// <summary>
        /// 添加同步的订阅者。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name">名称。</param>
        /// <param name="subscriber"></param>
        public void AddSyncSubscriber(Type type, string name, Delegate subscriber)
        {
            var list = _subscribers.GetOrAdd(name, () => new List<SubscribeDelegate>());
            if (list != null)
            {
                list.Add(new SyncSubscribeDelegate(type, subscriber));
            }
        }

        /// <summary>
        /// 添加异步的订阅者。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name">名称。</param>
        /// <param name="subscriber"></param>
        public void AddAsyncSubscriber(Type type, string name, Delegate subscriber)
        {
            var list = _subscribers.GetOrAdd(name, () => new List<SubscribeDelegate>());
            if (list != null)
            {
                list.Add(new AsyncSubscribeDelegate(type, subscriber));
            }
        }

        /// <summary>
        /// 移除订阅者。
        /// </summary>
        /// <param name="name">名称。</param>
        public void Remove(string name)
        {
            _subscribers.TryRemove(name, out _);
        }

        /// <summary>
        /// 清空所有订阅者。
        /// </summary>
        public void Clear()
        {
            _subscribers.Clear();
        }
    }
}
