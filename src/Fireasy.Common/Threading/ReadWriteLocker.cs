// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System;
using System.Threading;

namespace Fireasy.Common.Threading
{
    /// <summary>
    /// 提供在多线程环境中进行数据读取和写入的锁。无法继承此类。
    /// </summary>
    public sealed class ReadWriteLocker : DisposableBase
    {
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

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
            if (!_locker.IsReadLockHeld)
            {
                _locker.EnterReadLock();
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
                    _locker.ExitReadLock();
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
            if (!_locker.IsReadLockHeld)
            {
                _locker.EnterReadLock();
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
                    _locker.ExitReadLock();
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

            if (_locker.IsReadLockHeld)
            {
                _locker.ExitReadLock();
                if (!_locker.IsUpgradeableReadLockHeld)
                {
                    _locker.EnterUpgradeableReadLock();
                    isUpgradeable = true;
                }

                isMustReadHeld = true;
            }
            else if (!_locker.IsWriteLockHeld)
            {
                _locker.EnterWriteLock();
                isMustWriteHeld = true;
            }

            try
            {
                method();
            }
            catch
            {
            }
            finally
            {
                if (isMustReadHeld)
                {
                    if (isUpgradeable)
                    {
                        _locker.ExitUpgradeableReadLock();
                    }

                    _locker.EnterReadLock();
                }
                else if (isMustWriteHeld)
                {
                    _locker.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        protected override bool Dispose(bool disposing)
        {
            _locker.Dispose();

            return base.Dispose(disposing);
        }
    }
}
