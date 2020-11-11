// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using System;
using System.Threading;

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 匿名委托任务运行器。
    /// </summary>
    public class DefaultAnonymousTaskRunner : DisposableBase, ITaskRunner
    {
        private readonly Action<IServiceProvider> _executor;
        private Timer _timer;
        private readonly TimeSpan _delay;
        private readonly TimeSpan _period;
        private readonly TaskExecuteContext _context;

        /// <summary>
        /// 初始化 <see cref="DefaultAsyncTaskRunner{T}"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="delay">延迟时间。</param>
        /// <param name="period">执行触发间隔时间。</param>
        /// <param name="context">执行上下文对象。</param>
        public DefaultAnonymousTaskRunner(TimeSpan delay, TimeSpan period, Action<IServiceProvider> executor, TaskExecuteContext context)
        {
            _delay = delay;
            _period = period;
            _executor = executor;
            _context = context;
        }

        /// <summary>
        /// 开始运行。
        /// </summary>
        public void Start()
        {
            if (_timer == null)
            {
                _timer = new Timer(o =>
                {
                    using var scope = _context.ServiceProvider.TryCreateScope();
                    _executor(scope.ServiceProvider);

                    if (_period.TotalMilliseconds <= 0)
                    {
                        Stop();
                    }
                }, null, _delay, _period);
            }
        }

        /// <summary>
        /// 停止运行。
        /// </summary>
        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Change(TimeSpan.FromMilliseconds(-1.0), TimeSpan.FromMilliseconds(1.0));
            }
        }

        protected override bool Dispose(bool disposing)
        {
            _timer?.Dispose();
            _timer = null;

            return base.Dispose(disposing);
        }
    }

    /// <summary>
    /// 缺省的任务运行器。
    /// </summary>
    public class DefaultTaskRunner : DisposableBase, ITaskRunner
    {
        private readonly ITaskExecutor _executor;
        private Timer _timer;
        private readonly TimeSpan _delay;
        private readonly TimeSpan _period;
        private readonly TaskExecuteContext _context;

        /// <summary>
        /// 初始化 <see cref="DefaultTaskRunner{T}"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="delay">延迟时间。</param>
        /// <param name="period">执行触发间隔时间。</param>
        /// <param name="context">执行上下文对象。</param>
        public DefaultTaskRunner(TimeSpan delay, TimeSpan period, ITaskExecutor executor, TaskExecuteContext context)
        {
            _delay = delay;
            _period = period;
            _executor = executor;
            _context = context;
        }

        /// <summary>
        /// 开始运行。
        /// </summary>
        public void Start()
        {
            if (_timer == null)
            {
                _timer = new Timer(o =>
                {
                    using var scope = _context.ServiceProvider.TryCreateScope();
                    var context = new TaskExecuteContext(scope.ServiceProvider, _context.Arguments, _context.CancellationToken);
                    _executor.Execute(context);
                }, null, _delay, _period);
            }
        }

        /// <summary>
        /// 停止运行。
        /// </summary>
        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Change(TimeSpan.FromMilliseconds(-1.0), TimeSpan.FromMilliseconds(1.0));
            }
        }

        protected override bool Dispose(bool disposing)
        {
            _timer?.Dispose();
            _timer = null;

            return base.Dispose(disposing);
        }
    }
}
