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
using System.Threading;

namespace Fireasy.Common.ComponentModel
{
    /// <summary>
    /// 表示可缓冲的对象。
    /// </summary>
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
        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private readonly Func<T> creator;

        private readonly int maxSize;
        private int count;

        /// <summary>
        /// 初始化 <see cref="ObjectPool{T}"/> 类的新实例。
        /// </summary>
        /// <param name="creator">创建对象的函数。</param>
        /// <param name="maxSize">最大对象数。</param>
        public ObjectPool(Func<T> creator, int maxSize = 100)
        {
            this.creator = creator;
            this.maxSize = maxSize;
        }

        /// <summary>
        /// 从缓冲池中返回一个对象，如果缓冲池为空或对象已用尽，则创建一个新对象。
        /// </summary>
        /// <returns></returns>
        public virtual T Rent()
        {
            if (queue.TryDequeue(out var obj))
            {
                Interlocked.Decrement(ref count);
                OnRent(obj);
                return obj;
            }

            obj = creator();
            return obj;
        }

        /// <summary>
        /// 将对象归还到缓冲池中。如果缓冲池已满，则舍弃该对象。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual bool Return(T obj)
        {
            Guard.ArgumentNull(obj, nameof(obj));

            if (Interlocked.Increment(ref count) <= maxSize)
            {
                obj.SetPool(this);
                OnReturn(obj);
                queue.Enqueue(obj);
                return true;
            }

            Interlocked.Decrement(ref count);
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

        protected override void Dispose(bool disposing)
        {
            while (queue.TryDequeue(out var obj))
            {
                obj.SetPool(null);
                obj.TryDispose();
            }
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
