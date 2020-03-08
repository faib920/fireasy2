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
using System.Threading.Tasks;

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 缺省的异步任务运行器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultAsyncTaskRunner : DisposeableBase, ITaskRunner
    {
        private readonly IAsyncTaskExecutor executor;
        private Timer timer;
        private readonly CancellationTokenSource stopToken;
        private readonly TimeSpan delay;
        private readonly TimeSpan period;
        private readonly TaskExecuteContext context;

        /// <summary>
        /// 初始化 <see cref="DefaultAsyncTaskRunner{T}"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="delay">延迟时间。</param>
        /// <param name="period">执行触发间隔时间。</param>
        /// <param name="context">执行上下文对象。</param>
        public DefaultAsyncTaskRunner(TimeSpan delay, TimeSpan period, IAsyncTaskExecutor executor, TaskExecuteContext context)
        {
            this.delay = delay;
            this.period = period;
            this.executor = executor;
            this.context = context;
            stopToken = new CancellationTokenSource();
        }

        /// <summary>
        /// 开始运行。
        /// </summary>
        public void Start()
        {
            if (timer == null)
            {
                timer = new Timer(o =>
                {
                    Tracer.Debug($"The Task '{executor.GetType()}' Executing.");
                    Task.WaitAll(executor.ExecuteAsync(context, stopToken.Token));
                    Tracer.Debug($"The Task '{executor.GetType()}' Completed.");
                }, null, delay, period);
            }
        }

        /// <summary>
        /// 停止运行。
        /// </summary>
        public void Stop()
        {
            if (timer != null)
            {
                timer.Change(TimeSpan.FromMilliseconds(-1.0), TimeSpan.FromMilliseconds(1.0));
            }
        }

        protected override void Dispose(bool disposing)
        {
            timer?.Dispose();
            timer = null;
            stopToken.Cancel();
        }
    }
}
