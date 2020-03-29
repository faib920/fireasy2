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

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 缺省的任务调度管理器。
    /// </summary>
    public class DefaultTaskScheduler : DisposeableBase, ITaskScheduler, IServiceProviderAccessor
    {
        /// <summary>
        /// 获取 <see cref="DefaultTaskScheduler"/> 的静态实例。
        /// </summary>
        public readonly static DefaultTaskScheduler Instance = new DefaultTaskScheduler().TryUseContainer();

        private readonly List<ITaskRunner> runners = new List<ITaskRunner>();
        private readonly CancellationTokenSource stopToken = new CancellationTokenSource();

        /// <summary>
        /// 初始化 <see cref="DefaultTaskScheduler"/> 类的新实例。
        /// </summary>
        public DefaultTaskScheduler()
        {
        }

        /// <summary>
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 获取预先安排的执行任务队列。
        /// </summary>
        public Queue<TaskExecutorDefiniton> PreTasks { get; } = new Queue<TaskExecutorDefiniton>();

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
            var context = new TaskExecuteContext(ServiceProvider, options.Arguments, stopToken.Token);
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
            var context = new TaskExecuteContext(ServiceProvider, options.Arguments, stopToken.Token);
            AddRunner(new DefaultAsyncTaskRunner(options.Delay, options.Period, executor, context));
        }

        /// <summary>
        /// 启动启动任务调度器。
        /// </summary>
        public void Start()
        {
            while (PreTasks.Count > 0)
            {
                TaskRunHelper.Run(this, PreTasks.Dequeue());
            }
        }

        /// <summary>
        /// 停止启动任务调度器。
        /// </summary>
        public void Stop()
        {
            stopToken.Cancel();
            runners.ForEach(s => s.Stop());
        }

        protected override bool Dispose(bool disposing)
        {
            runners.ForEach(s => s.TryDispose());
            runners.Clear();

            stopToken.Dispose();

            return base.Dispose(disposing);
        }

        private void AddRunner(ITaskRunner runner)
        {
            runner.Start();
            runners.Add(runner);
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
