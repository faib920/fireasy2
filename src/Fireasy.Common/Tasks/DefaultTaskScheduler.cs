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
using System.Threading;
using System.Threading.Tasks;
#endif
using System.Collections.Generic;

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 缺省的任务调度管理器。
    /// </summary>
    public class DefaultTaskScheduler : DisposeableBase, ITaskScheduler
    {
        /// <summary>
        /// 获取 <see cref="DefaultTaskScheduler"/> 的静态实例。
        /// </summary>
        public readonly static DefaultTaskScheduler Instance = new DefaultTaskScheduler();

        private readonly List<ITaskRunner> runners = new List<ITaskRunner>();
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// 初始化 <see cref="DefaultTaskScheduler"/> 类的新实例。
        /// </summary>
        public DefaultTaskScheduler()
        {
        }

#if NETSTANDARD
        /// <summary>
        /// 初始化 <see cref="DefaultTaskScheduler"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="definitions"></param>
        public DefaultTaskScheduler(IServiceProvider serviceProvider, IEnumerable<TaskExecutorDefiniton> definitions)
        {
            this.serviceProvider = serviceProvider;

            if (definitions == null)
            {
                return;
            }

            foreach (var def in definitions)
            {
                var executor = serviceProvider.GetService(def.ExecutorType);
                if (executor is ITaskExecutor baseExecutor)
                {
                    AddRunner(new DefaultTaskRunner(def.Delay, def.Period, baseExecutor, new TaskExecuteContext(serviceProvider)));
                }
                else if (executor is IAsyncTaskExecutor asyncExecutor)
                {
                    AddRunner(new DefaultAsyncTaskRunner(def.Delay, def.Period, asyncExecutor, new TaskExecuteContext(serviceProvider)));
                }
            }
        }
#endif

        /// <summary>
        /// 启动一个任务执行器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options">启动选项。</param>
        public void Start<T>(StartOptions<T> options) where T : ITaskExecutor
        {
            if (!(typeof(T).New(serviceProvider) is ITaskExecutor executor))
            {
                throw new Exception();
            }

            options.Initializer?.Invoke((T)executor);
            var context = new TaskExecuteContext(serviceProvider, options.Arguments);
            AddRunner(new DefaultTaskRunner(options.Delay, options.Period, executor, context));
        }

        /// <summary>
        /// 启动一个异步的任务执行器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options">启动选项。</param>
        public void StartAsync<T>(StartOptions<T> options) where T : IAsyncTaskExecutor
        {
            if (!(typeof(T).New(serviceProvider) is IAsyncTaskExecutor executor))
            {
                throw new Exception();
            }

            options.Initializer?.Invoke((T)executor);
            var context = new TaskExecuteContext(serviceProvider, options.Arguments);
            AddRunner(new DefaultAsyncTaskRunner(options.Delay, options.Period, executor, context));
        }

        /// <summary>
        /// 停止任务执行器的运行。
        /// </summary>
        public void Stop()
        {
            runners.ForEach(s => s.Stop());
        }

        protected override void Dispose(bool disposing)
        {
            runners.ForEach(s => s.TryDispose());
            runners.Clear();
        }

        private void AddRunner(ITaskRunner runner)
        {
            runner.Start();
            runners.Add(runner);
        }

#if NETSTANDARD
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
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
