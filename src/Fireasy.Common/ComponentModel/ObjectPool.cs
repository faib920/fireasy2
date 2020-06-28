// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using Fireasy.Common.MultiTenancy;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Fireasy.Common.ComponentModel
{
    /// <summary>
    /// 表示可缓冲的对象。
    /// </summary>
    [IgnoreRegister]
    public interface IObjectPoolable
    {
        /// <summary>
        /// 设置缓冲池实例。
        /// </summary>
        /// <param name="pool"></param>
        void SetPool(IObjectPool pool);
    }

    /// <summary>
    /// 定义对象缓冲池的相关方法。
    /// </summary>
    public interface IObjectPool
    {
        /// <summary>
        /// 从缓冲池中返回一个对象。
        /// </summary>
        /// <returns></returns>
        object Rent();

        /// <summary>
        /// 将对象归还到缓冲池中。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Return(object obj);
    }

    /// <summary>
    /// 默认的对象缓冲池。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : DisposeableBase, IObjectPool where T : class, IObjectPoolable
    {
        private readonly SafetyDictionary<string, InternalQueue> queueDict = new SafetyDictionary<string, InternalQueue>();
        private readonly Func<T> creator;

        private readonly int maxSize;
        private Timer timer = null;
        private TimeSpan? idleTime = TimeSpan.FromMinutes(1);
        private readonly ITenancyProvider<ObjectPoolTenancyInfo> tenancyProvider;

        private class InternalQueue
        {
            private DateTime? lastAccessTime = null;
            private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
            private readonly int maxSize;
            private int count;

            public InternalQueue(ObjectPool<T> pool, string key)
            {
                maxSize = pool.maxSize;
                Key = key;
            }

            public int Count  => count;

            public string Key { get; }

            public bool TryDequeue(out T obj)
            {
                if (queue.TryDequeue(out obj))
                {
                    Interlocked.Decrement(ref count);
                    return true;
                }

                return false;
            }

            public bool TryEnqueue(T obj)
            {
                if (Interlocked.Increment(ref count) <= maxSize)
                {
                    lastAccessTime = DateTime.Now;

                    queue.Enqueue(obj);

                    return true;
                }

                Interlocked.Decrement(ref count);

                return false;
            }

            public bool TryIdle(TimeSpan? idleTime, Action<T> idleHandler)
            {
                var _savedLastAccessTime = lastAccessTime.Value;
                if (DateTime.Now - lastAccessTime.Value > idleTime.Value)
                {
                    while (TryDequeue(out T obj))
                    {
                        idleHandler?.Invoke(obj);

                        if (_savedLastAccessTime != lastAccessTime)
                        {
                            break;
                        }
                    }

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 初始化 <see cref="ObjectPool{T}"/> 类的新实例。
        /// </summary>
        /// <param name="creator">创建对象的函数。</param>
        /// <param name="maxSize">最大对象数。</param>
        /// <param name="tenancyProvider">多租户信息提供者。</param>
        public ObjectPool(Func<T> creator, int maxSize = 100, ITenancyProvider<ObjectPoolTenancyInfo> tenancyProvider = null)
        {
            this.creator = creator;
            this.maxSize = maxSize;
            this.tenancyProvider = tenancyProvider;

            StartIdleCheckThread();
        }

        /// <summary>
        /// 获取或设置空闲清理时间，默认为 1 分钟。
        /// </summary>
        public virtual TimeSpan? IdleTime
        {
            get
            {
                return idleTime;
            }
            set
            {
                idleTime = value;

                if (idleTime == null)
                {
                    timer.Change(TimeSpan.MaxValue, TimeSpan.MaxValue);
                }
                else
                {
                    timer.Change(idleTime.Value, idleTime.Value);
                }
            }
        }

        /// <summary>
        /// 从缓冲池中返回一个对象，如果缓冲池为空或对象已用尽，则创建一个新对象。
        /// </summary>
        /// <returns></returns>
        public virtual T Rent()
        {
            return Rent(creator, null);
        }

        /// <summary>
        /// 从缓冲池中返回一个对象，如果缓冲池为空或对象已用尽，则创建一个新对象。
        /// </summary>
        /// <param name="creator">创建新对象的函数。</param>
        /// <param name="initializer">从池里拿出时对象的初始化函数。</param>
        /// <returns></returns>
        public virtual T Rent(Func<T> creator, Action<T> initializer)
        {
            var queue = GetQueue();
            if (queue.TryDequeue(out T obj))
            {
                Tracer.Debug($"Rent {typeof(T).Name} from the pool (named:{queue.Key}, count:{queue.Count}).");
                OnRent(obj);
                initializer?.Invoke(obj);
                return obj;
            }

            var obj1 = creator();
            obj1.SetPool(this);
            Tracer.Debug($"Generate a new object of {typeof(T).Name}.");
            return obj1;
        }

        /// <summary>
        /// 将对象归还到缓冲池中。如果缓冲池已满，则舍弃该对象。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual bool Return(T obj)
        {
            Guard.ArgumentNull(obj, nameof(obj));
            var queue = GetQueue();

            if (queue.TryEnqueue(obj))
            {
                Tracer.Debug($"Return {typeof(T).Name} back to the pool (named:{queue.Key}, count:{queue.Count}).");

                obj.SetPool(this);
                OnReturn(obj);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 从缓冲池中返回一个对象时的通知。
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnRent(T obj)
        {
        }

        /// <summary>
        /// 将对象归还到缓冲池中时的通知。
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnReturn(T obj)
        {
        }

        protected override bool Dispose(bool disposing)
        {
            foreach (var q in queueDict)
            {
                while (q.Value.TryDequeue(out T obj))
                {
                    obj.SetPool(null);
                    obj.TryDispose();
                }
            }

            return base.Dispose(disposing);
        }

        private InternalQueue GetQueue()
        {
            var tenancy = tenancyProvider?.GetTenancyInfo() ?? ObjectPoolTenancyInfo.Default;
            return queueDict.GetOrAdd(tenancy.Key, k => new InternalQueue(this, k));
        }

        private void StartIdleCheckThread()
        {
            timer = new Timer(o =>
            {
                foreach (var q in queueDict)
                {
                    q.Value.TryIdle(IdleTime, obj =>
                    {
                        Tracer.Debug($"Take back the idle {typeof(T).Name} from the pool (named:{q.Key}, count:{q.Value.Count}).");
                        obj.SetPool(null);
                        obj.TryDispose();
                    });
                }
            }, null, IdleTime.Value, IdleTime.Value);
        }

        object IObjectPool.Rent()
        {
            return Rent();
        }

        bool IObjectPool.Return(object obj)
        {
            return Return((T)obj);
        }
    }
}
