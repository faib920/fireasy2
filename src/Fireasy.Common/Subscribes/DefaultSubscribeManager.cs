// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System;
#if !NET35
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Threading;

namespace Fireasy.Common.Subscribes
{
    /// <summary>
    /// 默认的订阅管理器。
    /// </summary>
    public class DefaultSubscribeManager : ISubscribeManager
    {
        private static SafetyDictionary<Type, List<Delegate>> subscribers = new SafetyDictionary<Type, List<Delegate>>();
#if NET35
        private Queue<object> queue = new Queue<object>();
#else
        private ConcurrentQueue<object> queue = new ConcurrentQueue<object>();
#endif
        private Thread thread = null;

        /// <summary>
        /// 缺省实例。
        /// </summary>
        public static ISubscribeManager Instance = new DefaultSubscribeManager();

        protected DefaultSubscribeManager()
        {
            thread = new Thread(new ThreadStart(ProcessQueue)) { IsBackground = true };
        }

        private void ProcessQueue()
        {
            while (true)
            {
                if (queue.Count == 0)
                {
                    Thread.Sleep(1000);
                }

#if !NET35
                if (queue.TryDequeue(out object obj))
#else
                object obj = null;
                if (queue.Count > 0 && (obj = queue.Dequeue()) != null)
#endif
                {
                    if (obj != null && subscribers.TryGetValue(obj.GetType(), out List<Delegate> list) && list != null)
                    {
                        list.ForEach(s => s.DynamicInvoke(obj));
                    }
                }
            }
        }

        /// <summary>
        /// 向管理器发送主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        public void Publish<TSubject>(TSubject subject) where TSubject : class
        {
            queue.Enqueue(subject);

            if (thread.ThreadState != ThreadState.Background)
            {
                thread.Start();
            }
        }

        /// <summary>
        /// 添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber<TSubject>(Action<TSubject> subscriber) where TSubject : class
        {
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            var list = subscribers.GetOrAdd(typeof(TSubject), () => new List<Delegate>());
            if (list != null)
            {
                list.Add(subscriber);
            }
        }

        /// <summary>
        /// 添加一个订阅方法。
        /// </summary>
        /// <param name="subjectType">主题的类型。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber(Type subjectType, Delegate subscriber)
        {
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            var list = subscribers.GetOrAdd(subjectType, () => new List<Delegate>());
            if (list != null)
            {
                list.Add(subscriber);
            }
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
            subscribers.TryRemove(subjectType, out List<Delegate> delegates);
        }

        void ISubscribeManager.Publish(string channel, byte[] data)
        {
            throw new NotImplementedException();
        }

        void ISubscribeManager.AddSubscriber(string channel, Action<byte[]> subscriber)
        {
            throw new NotImplementedException();
        }

        void ISubscribeManager.RemoveSubscriber(string channel)
        {
            throw new NotImplementedException();
        }
    }
}
