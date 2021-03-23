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
        /// 获取池标识。
        /// </summary>
        string PoolName { get; }

        /// <summary>
        /// 设置缓冲池实例。
        /// </summary>
        /// <param name="poolName"></param>
        void SetPool(string poolName);

        /// <summary>
        /// 从缓冲池拿出实例时通知。
        /// </summary>
        void OnRent();

        /// <summary>
        /// 将对象还回缓冲池时通知。
        /// </summary>
        void OnReturn();
    }

    /// <summary>
    /// 表示缓冲池通知链。
    /// </summary>
    public interface IObjectPoolNotifyChain
    {
        /// <summary>
        /// 将对象还回缓冲池时通知。
        /// </summary>
        void OnReturn();
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
    public class ObjectPool<T> : DisposableBase, IObjectPool where T : class, IObjectPoolable
    {
        private readonly SafetyDictionary<string, InternalQueue> _queueDict = new SafetyDictionary<string, InternalQueue>();
        private readonly Func<T> _creator;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly int _maxSize;
        private Timer _timer = null;
        private TimeSpan? _idleTime = TimeSpan.FromMinutes(1);
        private readonly ITenancyProvider<ObjectPoolTenancyInfo> _tenancyProvider;

        private class InternalQueue
        {
            private DateTime? _lastAccessTime = null;
            private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
            private readonly int _maxSize;
            private int _count;

            public InternalQueue(ObjectPool<T> pool, string key)
            {
                _maxSize = pool._maxSize;
                Key = key;
            }

            public int Count => _count;

            public string Key { get; }

            public bool TryDequeue(out T obj)
            {
                _lastAccessTime = DateTime.Now;

                if (_queue.TryDequeue(out obj))
                {
                    Interlocked.Decrement(ref _count);
                    return true;
                }

                return false;
            }

            public bool TryEnqueue(T obj)
            {
                if (Interlocked.Increment(ref _count) <= _maxSize)
                {
                    _lastAccessTime = DateTime.Now;

                    _queue.Enqueue(obj);

                    return true;
                }

                Interlocked.Decrement(ref _count);

                return false;
            }

            public bool TryIdle(TimeSpan? idleTime, Action<T> idleHandler)
            {
                if (_lastAccessTime == null)
                {
                    return false;
                }

                var _savedLastAccessTime = _lastAccessTime.Value;
                if (DateTime.Now - _lastAccessTime.Value > idleTime.Value)
                {
                    while (TryDequeue(out T obj))
                    {
                        idleHandler?.Invoke(obj);

                        if (_savedLastAccessTime != _lastAccessTime)
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
        /// <param name="serviceProvider"></param>
        /// <param name="creator">创建对象的函数。</param>
        /// <param name="maxSize">最大对象数。</param>
        public ObjectPool(IServiceProvider serviceProvider, Func<T> creator, int maxSize = 100)
        {
            _creator = creator;
            _maxSize = maxSize;
            _tenancyProvider = serviceProvider.TryGetService<ITenancyProvider<ObjectPoolTenancyInfo>>();

            StartIdleCheckThread();
        }

        /// <summary>
        /// 获取或设置空闲清理时间，默认为 1 分钟。
        /// </summary>
        public virtual TimeSpan? IdleTime
        {
            get
            {
                return _idleTime;
            }
            set
            {
                _idleTime = value;

                if (_idleTime == null)
                {
                    _timer.Change(TimeSpan.MaxValue, TimeSpan.MaxValue);
                }
                else
                {
                    _timer.Change(_idleTime.Value, _idleTime.Value);
                }
            }
        }

        /// <summary>
        /// 从缓冲池中返回一个对象，如果缓冲池为空或对象已用尽，则创建一个新对象。
        /// </summary>
        /// <returns></returns>
        public virtual T Rent()
        {
            return Rent(_creator, null);
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
                obj.OnRent();
                return obj;
            }

            var obj1 = creator();
            obj1.SetPool(queue.Key);
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
            var queue = GetQueue(obj.PoolName);

            if (queue.TryEnqueue(obj))
            {
                Tracer.Debug($"Return {typeof(T).Name} back to the pool (named:{queue.Key}, count:{queue.Count}).");

                obj.OnReturn();
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
            foreach (var q in _queueDict)
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
            var tenancy = _tenancyProvider?.Resolve(ObjectPoolTenancyInfo.Default) ?? ObjectPoolTenancyInfo.Default;
            return _queueDict.GetOrAdd(tenancy.Key, k => new InternalQueue(this, k));
        }

        private InternalQueue GetQueue(string key)
        {
            return _queueDict.GetOrAdd(key, k => new InternalQueue(this, k));
        }

        private void StartIdleCheckThread()
        {
            _timer = new Timer(o =>
            {
                foreach (var q in _queueDict)
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
