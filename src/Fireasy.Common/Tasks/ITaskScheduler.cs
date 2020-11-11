// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Microsoft.Extensions.Hosting;
#endif
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 定义任务调度管理器。
    /// </summary>
    public interface ITaskScheduler
#if NETSTANDARD
        : IHostedService
#endif
    {
        /// <summary>
        /// 启动一个任务执行器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options">启动选项。</param>
        void StartExecutor<T>(StartOptions<T> options) where T : ITaskExecutor;

        /// <summary>
        /// 启动一个异步的任务执行器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options">启动选项。</param>
        void StartExecutorAsync<T>(StartOptions<T> options) where T : IAsyncTaskExecutor;

        /// <summary>
        /// 启动一个方法。
        /// </summary>
        /// <param name="options"></param>
        /// <param name="executor"></param>
        void Start(StartOptions options, Action<IServiceProvider> executor);

        /// <summary>
        /// 启动一个异步方法。
        /// </summary>
        /// <param name="options"></param>
        /// <param name="executor"></param>
        void StartAsync(StartOptions options, Func<IServiceProvider, CancellationToken, Task> executor);

        /// <summary>
        /// 启动任务调度器。
        /// </summary>
        void Start();

        /// <summary>
        /// 停止任务调度器。
        /// </summary>
        void Stop();
    }
}
