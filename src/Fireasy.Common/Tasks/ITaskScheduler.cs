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
        /// <param name="option">启动选项。</param>
        void Start<T>(StartOptions<T> option) where T : ITaskExecutor;

        /// <summary>
        /// 启动一个异步的任务执行器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="option">启动选项。</param>
        void StartAsync<T>(StartOptions<T> option) where T : IAsyncTaskExecutor;

        /// <summary>
        /// 停止任务执行器的运行。
        /// </summary>
        void Stop();
    }
}
