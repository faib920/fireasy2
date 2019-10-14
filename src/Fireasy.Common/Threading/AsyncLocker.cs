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
            private readonly AsyncLocker toRelease;

            internal Releaser(AsyncLocker toRelease)
            {
                this.toRelease = toRelease;
            }

            public void Dispose()
            {
                if (toRelease != null)
                {
                    toRelease.semaphore.Release();
                }
            }
        }

        private readonly AsyncSemaphore semaphore;

        private readonly Task<Releaser> releaser;

        public AsyncLocker()
        {
            semaphore = new AsyncSemaphore(1);
            releaser = Task.FromResult(new Releaser(this));
        }

        /// <summary>
        /// 锁定，并返回一个 <see cref="Releaser"/> 对象。
        /// </summary>
        /// <returns></returns>
        public Task<Releaser> LockAsync()
        {
            var task = semaphore.WaitAsync();
            if (!task.IsCompleted)
            {
                return task.ContinueWith((Task _, object state) => new Releaser((AsyncLocker)state), this, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            return releaser;
        }

        /// <summary>
        /// 锁定任务的执行。
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task LockAsync(Task task)
        {
            using (var releaser = LockAsync())
            {
                await task;
            }
        }
    }

    /// <summary>
    /// 异步信号。
    /// </summary>
    internal class AsyncSemaphore
    {
        private static readonly Task completed = Task.FromResult(result: true);

        private readonly Queue<TaskCompletionSource<bool>> waiters = new Queue<TaskCompletionSource<bool>>();

        private int currentCount;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException("initialCount");
            }

            currentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (waiters)
            {
                if (currentCount > 0)
                {
                    currentCount--;
                    return completed;
                }

                TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                waiters.Enqueue(taskCompletionSource);
                return taskCompletionSource.Task;
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> taskCompletionSource = null;
            lock (waiters)
            {
                if (waiters.Count > 0)
                {
                    taskCompletionSource = waiters.Dequeue();
                }
                else
                {
                    currentCount++;
                }
            }

            taskCompletionSource?.SetResult(result: true);
        }
    }

}
