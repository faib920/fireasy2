// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Threading;

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 缺省的异步任务运行器。
    /// </summary>
    public class DefaultAsyncTaskRunner : DisposeableBase, ITaskRunner
    {
        private readonly IAsyncTaskExecutor executor;
        private Timer timer;
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
                    executor.ExecuteAsync(context);
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

        protected override bool Dispose(bool disposing)
        {
            timer?.Dispose();
            timer = null;

            return base.Dispose(disposing);
        }
    }
}
