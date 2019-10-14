// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Threading;

namespace Fireasy.Common.Threading
{
    /// <summary>
    /// 提供在多线程环境中进行数据读取和写入的锁。无法继承此类。
    /// </summary>
    public sealed class ReadWriteLocker : IDisposable
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private bool isDisposed;

        /// <summary>
        /// 静态实例。
        /// </summary>
        public static readonly ReadWriteLocker Instance = new ReadWriteLocker();

        /// <summary>
        /// 使用指定的方法进行读操作。
        /// </summary>
        /// <param name="method">一个方法，在此方法执行期间将被读锁定。</param>
        public void LockRead(Action method)
        {
            Guard.ArgumentNull(method, nameof(method));

            var isMustReadHeld = false;
            if (!locker.IsReadLockHeld)
            {
                locker.EnterReadLock();
                isMustReadHeld = true;
            }

            try
            {
                method();
            }
            finally
            {
                if (isMustReadHeld)
                {
                    locker.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// 使用指定的方法进行读操作。
        /// </summary>
        /// <param name="method">一个方法，在此方法执行期间将被读锁定。</param>
        /// <returns>方法 <paramref name="method"/> 返回的数据。</returns>
        public T LockRead<T>(Func<T> method)
        {
            Guard.ArgumentNull(method, nameof(method));

            var isMustReadHeld = false;
            if (!locker.IsReadLockHeld)
            {
                locker.EnterReadLock();
                isMustReadHeld = true;
            }

            try
            {
                return method();
            }
            finally
            {
                if (isMustReadHeld)
                {
                    locker.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// 使用指定的方法进行写操作。
        /// </summary>
        /// <param name="method">一个方法，在此方法执行期间将被写锁定。</param>
        public void LockWrite(Action method)
        {
            Guard.ArgumentNull(method, nameof(method));

            var isMustReadHeld = false;
            var isMustWriteHeld = false;
            var isUpgradeable = false;

            if (locker.IsReadLockHeld)
            {
                locker.ExitReadLock();
                if (!locker.IsUpgradeableReadLockHeld)
                {
                    locker.EnterUpgradeableReadLock();
                    isUpgradeable = true;
                }

                isMustReadHeld = true;
            }
            else if (!locker.IsWriteLockHeld)
            {
                locker.EnterWriteLock();
                isMustWriteHeld = true;
            }

            try
            {
                method();
            }
            finally
            {
                if (isMustReadHeld)
                {
                    if (isUpgradeable)
                    {
                        locker.ExitUpgradeableReadLock();
                    }

                    locker.EnterReadLock();
                }
                else if (isMustWriteHeld)
                {
                    locker.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        private void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                locker.Dispose();
                Debug.WriteLine("The instance of ReaderWriterLockSlim is disposed.");
            }

            isDisposed = true;
        }

        /// <summary>
        /// 释放对象所占用的所有资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
