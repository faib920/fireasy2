// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
#if NETSTANDARD
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
#endif
using System.Collections.Generic;
using System.Threading;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Ioc;
using System.Threading.Tasks;

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 缺省的任务调度管理器。
    /// </summary>
    public class DefaultTaskScheduler : DisposableBase, ITaskScheduler, IServiceProviderAccessor
    {
        /// <summary>
        /// 获取 <see cref="DefaultTaskScheduler"/> 的静态实例。
        /// </summary>
        public readonly static DefaultTaskScheduler Instance = new DefaultTaskScheduler();

        private bool _isRunning;
        private readonly List<ITaskRunner> _runners = new List<ITaskRunner>();
        private readonly CancellationTokenSource _stopToken = new CancellationTokenSource();

        /// <summary>
        /// 初始化 <see cref="DefaultTaskScheduler"/> 类的新实例。
        /// </summary>
        public DefaultTaskScheduler()
            : this(ContainerUnity.GetContainer())
        {
        }

        /// <summary>
        /// 初始化 <see cref="DefaultTaskScheduler"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        public DefaultTaskScheduler(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 启动一个任务执行器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options">启动选项。</param>
        public void StartExecutor<T>(StartOptions<T> options) where T : ITaskExecutor
        {
            if (!(typeof(T).New(ServiceProvider) is ITaskExecutor executor))
            {
                throw new Exception();
            }

            options.Initializer?.Invoke((T)executor);
            var context = new TaskExecuteContext(ServiceProvider, options.Arguments, _stopToken.Token);
            AddRunner(new DefaultTaskRunner(options.Delay, options.Period, executor, context));
        }

        /// <summary>
        /// 启动一个异步的任务执行器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options">启动选项。</param>
        public void StartExecutorAsync<T>(StartOptions<T> options) where T : IAsyncTaskExecutor
        {
            if (!(typeof(T).New(ServiceProvider) is IAsyncTaskExecutor executor))
            {
                throw new Exception();
            }

            options.Initializer?.Invoke((T)executor);
            var context = new TaskExecuteContext(ServiceProvider, options.Arguments, _stopToken.Token);
            AddRunner(new DefaultAsyncTaskRunner(options.Delay, options.Period, executor, context));
        }

        /// <summary>
        /// 启动一个方法。
        /// </summary>
        /// <param name="options"></param>
        /// <param name="executor"></param>
        public void Start(StartOptions options, Action<IServiceProvider> executor)
        {
            var context = new TaskExecuteContext(ServiceProvider, options.Arguments, _stopToken.Token);
            AddRunner(new DefaultAnonymousTaskRunner(options.Delay, options.Period, executor, context));
        }

        /// <summary>
        /// 启动一个异步方法。
        /// </summary>
        /// <param name="options"></param>
        /// <param name="executor"></param>
        public void StartAsync(StartOptions options, Func<IServiceProvider, CancellationToken, Task> executor)
        {
            var context = new TaskExecuteContext(ServiceProvider, options.Arguments, _stopToken.Token);
            AddRunner(new DefaultAnonymousAsyncTaskRunner(options.Delay, options.Period, executor, context));
        }

        /// <summary>
        /// 启动启动任务调度器。
        /// </summary>
        public void Start()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
        }

        /// <summary>
        /// 停止启动任务调度器。
        /// </summary>
        public void Stop()
        {
            _stopToken.Cancel();
            _runners.ForEach(s => s.Stop());
            _isRunning = false;
        }

        protected override bool Dispose(bool disposing)
        {
            _runners.ForEach(s => s.TryDispose());
            _runners.Clear();

            _stopToken.Dispose();

            return base.Dispose(disposing);
        }

        private void AddRunner(ITaskRunner runner)
        {
            runner.Start();
            _runners.Add(runner);
        }

#if NETSTANDARD
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            Start();
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            Stop();
            return Task.CompletedTask;
        }
#endif
    }
}
