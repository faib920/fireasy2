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
using System.Collections.Generic;

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
        /// 获取预先安排的执行任务队列。
        /// </summary>
        Queue<TaskExecutorDefiniton> PreTasks { get; }

        /// <summary>
        /// 启动一个任务执行器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="option">启动选项。</param>
        void StartExecutor<T>(StartOptions<T> option) where T : ITaskExecutor;

        /// <summary>
        /// 启动一个异步的任务执行器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="option">启动选项。</param>
        void StartExecutorAsync<T>(StartOptions<T> option) where T : IAsyncTaskExecutor;

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
