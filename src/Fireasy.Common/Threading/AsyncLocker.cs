// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Threading
{
    /// <summary>
    /// 为异步环境下提供一种锁机制。
    /// </summary>
    public class AsyncLocker
    {
        /// <summary>
        /// 异步锁释放者。
        /// </summary>
        public struct Releaser : IDisposable
        {
            private readonly AsyncLocker _toRelease;

            internal Releaser(AsyncLocker toRelease)
            {
                _toRelease = toRelease;
            }

            public void Dispose()
            {
                if (_toRelease != null)
                {
                    _toRelease._semaphore.Release();
                }
            }
        }

        private readonly AsyncSemaphore _semaphore;
        private readonly Task<Releaser> _releaser;

        public AsyncLocker()
        {
            _semaphore = new AsyncSemaphore(1);
            _releaser = Task.FromResult(new Releaser(this));
        }

        /// <summary>
        /// 锁定，并返回一个 <see cref="Releaser"/> 对象。
        /// </summary>
        /// <returns></returns>
        public Task<Releaser> LockAsync()
        {
            var task = _semaphore.WaitAsync();
            if (!task.IsCompleted)
            {
                return task.ContinueWith((Task _, object state) => new Releaser((AsyncLocker)state), this, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            return _releaser;
        }

        /// <summary>
        /// 锁定任务的执行。
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task LockAsync(Task task)
        {
            using var releaser = LockAsync();
            await task;
        }
    }

    /// <summary>
    /// 异步信号。
    /// </summary>
    internal class AsyncSemaphore
    {
        private static readonly Task _completed = Task.FromResult(result: true);
        private readonly Queue<TaskCompletionSource<bool>> _waiters = new Queue<TaskCompletionSource<bool>>();
        private int _currentCount;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCount));
            }

            _currentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (_waiters)
            {
                if (_currentCount > 0)
                {
                    _currentCount--;
                    return _completed;
                }

                TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                _waiters.Enqueue(taskCompletionSource);
                return taskCompletionSource.Task;
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> taskCompletionSource = null;
            lock (_waiters)
            {
                if (_waiters.Count > 0)
                {
                    taskCompletionSource = _waiters.Dequeue();
                }
                else
                {
                    _currentCount++;
                }
            }

            taskCompletionSource?.SetResult(result: true);
        }
    }

}
