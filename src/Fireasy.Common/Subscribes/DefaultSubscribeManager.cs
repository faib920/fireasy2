// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Subscribes
{
    /// <summary>
    /// 默认的订阅管理器。
    /// </summary>
    public class DefaultSubscribeManager : ISubscribeManager
    {
        private static SafetyDictionary<string, List<Delegate>> subscribers = new SafetyDictionary<string, List<Delegate>>();
        private ConcurrentQueue<SubjectData> queue = new ConcurrentQueue<SubjectData>();
        private Thread thread = null;

        private class SubjectData
        {
            public SubjectData(Type type, object data)
            {
                Name = TopicHelper.GetTopicName(type);
                Data = data;
            }

            public SubjectData(string name, object data)
            {
                Name = name;
                Data = data;
            }

            public string Name { get; set; }

            public object Data { get; set; }
        }

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

                if (queue.TryDequeue(out SubjectData obj))
                {
                    if (obj != null && subscribers.TryGetValue(obj.Name, out List<Delegate> list) && list != null)
                    {
                        list.ForEach(s => s.DynamicInvoke(obj.Data));
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
            queue.Enqueue(new SubjectData(typeof(TSubject), subject));

            if (thread.ThreadState != ThreadState.Background)
            {
                thread.Start();
            }
        }

        /// <summary>
        /// 向管理器发送主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="name">主题名称。</param>
        /// <param name="subject">主题内容。</param>
        public void Publish<TSubject>(string name, TSubject subject) where TSubject : class
        {
            queue.Enqueue(new SubjectData(name, subject));

            if (thread.ThreadState != ThreadState.Background)
            {
                thread.Start();
            }
        }

        /// <summary>
        /// 异步方式向管理器发送主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task PublishAsync<TSubject>(TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            await Task.Run(() => Publish(subject), cancellationToken);
        }

        /// <summary>
        /// 异步方式向管理器发送主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="name">主题名称。</param>
        /// <param name="subject">主题内容。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task PublishAsync<TSubject>(string name, TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            await Task.Run(() => Publish(name, subject), cancellationToken);
        }

        /// <summary>
        /// 添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber<TSubject>(Action<TSubject> subscriber) where TSubject : class
        {
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            var list = subscribers.GetOrAdd(TopicHelper.GetTopicName(typeof(TSubject)), () => new List<Delegate>());
            if (list != null)
            {
                list.Add(subscriber);
            }
        }

        /// <summary>
        /// 添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="name">主题名称。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        public void AddSubscriber<TSubject>(string name, Action<TSubject> subscriber) where TSubject : class
        {
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            var list = subscribers.GetOrAdd(name, () => new List<Delegate>());
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

            var list = subscribers.GetOrAdd(TopicHelper.GetTopicName(subjectType), () => new List<Delegate>());
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
            subscribers.TryRemove(TopicHelper.GetTopicName(subjectType), out List<Delegate> delegates);
        }

        void ISubscribeManager.Publish(string name, byte[] data)
        {
            throw new NotImplementedException();
        }

        Task ISubscribeManager.PublishAsync(string name, byte[] data, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        void ISubscribeManager.AddSubscriber(string name, Action<byte[]> subscriber)
        {
            throw new NotImplementedException();
        }

        void ISubscribeManager.RemoveSubscriber(string name)
        {
            throw new NotImplementedException();
        }
    }
}
