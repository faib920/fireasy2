// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Common.Subscribe
{
    /// <summary>
    /// 订阅主题的管理器。
    /// </summary>
    public static class SubscribeManager
    {
        private static ConcurrentDictionary<Type, SubjectCache> subscribes = new ConcurrentDictionary<Type, SubjectCache>();

        /// <summary>
        /// 发布消息。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arguments">消息的参数。</param>
        public static void Publish<T>(params object[] arguments) where T : ISubject
        {
            if (subscribes.TryGetValue(typeof(T), out SubjectCache cache))
            {
                cache.Subject.Initialize(arguments);
                var subscribers = cache.Subject.Filter == null ?
                    cache.Subscribers : cache.Subscribers.Where(s => cache.Subject.Filter(s));
                SubscribePublisher.Publish(cache.Subject, subscribers);
            }
        }

        /// <summary>
        /// 将一个订阅者注册到指定的主题管理器中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>(ISubscriber subscriber) where T : ISubject
        {
            var subjectType = typeof(T);
            var lazy = new Lazy<SubjectCache>(() => new SubjectCache { Subject = subjectType.New<ISubject>() });

            var cache = subscribes.GetOrAdd(subjectType, s => lazy.Value);
            cache.AddSubscriber(subscriber);
        }

        /// <summary>
        /// 将一个订阅者从主题管理器中移除。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriber"></param>
        public static void UnRegister<T>(ISubscriber subscriber)
        {
            SubjectCache cache;
            if (subscribes.TryRemove(typeof(T), out cache))
            {
                cache.Subscribers.Clear();
            }
        }

        private class SubjectCache
        {
            internal SubjectCache()
            {
                Subscribers = new List<ISubscriber>();
            }

            internal List<ISubscriber> Subscribers { get; set; }

            internal ISubject Subject { get; set; }

            internal void AddSubscriber(ISubscriber subscriber)
            {
                if (!Subscribers.Contains(subscriber))
                {
                    Subscribers.Add(subscriber);
                }
            }

            internal void RemoveSubscriber(ISubscriber subscriber)
            {
                var index = Subscribers.IndexOf(subscriber);
                if (index != -1)
                {
                    Subscribers.RemoveAt(index);
                }
            }

            internal bool IsMatch(Type type)
            {
                return Subject.GetType() == type;
            }
        }
    }

    /// <summary>
    /// 订阅消息的发布器。
    /// </summary>
    internal class SubscribePublisher
    {
        /// <summary>
        /// 静态方法，发布订阅消息给订阅消息的订阅者。
        /// </summary>
        /// <param name="subject">订阅的主题。</param>
        /// <param name="subscribers">订阅该主题的订阅者列表。</param>
        internal static void Publish(ISubject subject, IEnumerable<ISubscriber> subscribers)
        {
#if !NET35
            ActionQueue.Push(() => subscribers.ForEach(s => s.Accept(subject)));
#else
            subscribers.ForEach(s => s.Accept(subject));
#endif
        }
    }
}
