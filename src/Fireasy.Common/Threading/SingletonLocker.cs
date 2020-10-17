// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Threading;

namespace Fireasy.Common.Threading
{
    /// <summary>
    /// 单例锁。
    /// </summary>
    public static class SingletonLocker
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 锁定 <paramref name="instance"/> 单例变量，使用默认的构造函数实例化。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">要锁定的单例变量。</param>
        /// <param name="timeout">超时时间。</param>
        /// <returns></returns>
        public static T Lock<T>(ref T instance, TimeSpan? timeout = null) where T : class, new()
        {
            return Lock<T>(ref instance, () => new T(), timeout);
        }

        /// <summary>
        /// 锁定 <paramref name="instance"/> 单例变量，避免多次实例化。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">要锁定的单例变量。</param>
        /// <param name="valueCreator">当变量为 null 时，使用此函数进行实例化。</param>
        /// <param name="timeout">超时时间。</param>
        /// <returns></returns>
        public static T Lock<T>(ref T instance, Func<T> valueCreator, TimeSpan? timeout = null) where T : class
        {
            if (instance != null)
            {
                return instance;
            }

            if (timeout == null)
            {
                _semaphore.Wait();
            }
            else
            {
                _semaphore.Wait(timeout.Value);
            }

            if (instance == null)
            {
                instance = valueCreator();
            }

            _semaphore.Release();

            return instance;
        }

        /// <summary>
        /// 使用一个对象来锁定 <paramref name="instance"/> 单例变量。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">要锁定的单例变量。</param>
        /// <param name="locker">用来上锁的对象。</param>
        /// <param name="valueCreator">当变量为 null 时，使用此函数进行实例化。</param>
        /// <returns></returns>
        public static T Lock<T>(ref T instance, object locker, Func<T> valueCreator)
        {
            if (instance != null)
            {
                return instance;
            }

            lock (locker)
            {
                if (instance == null)
                {
                    instance = valueCreator();
                }
            }

            return instance;
        }
    }
}
