// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Common.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fireasy.QuartzNet
{
    /// <summary>
    /// 异步的 <see cref="IJob"/> 包装器。
    /// </summary>
    public class AsyncJobWrapper : IJob
    {
        /// <summary>
        /// 执行任务。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            if (context.MergedJobDataMap["executor"] is IAsyncTaskExecutor executor)
            {
                var service = context.MergedJobDataMap["serviceProvider"] as IServiceProvider;
                var arguments = context.MergedJobDataMap["arguments"] as IDictionary<string, object>;
                var econtext = new TaskExecuteContext(service, arguments);
                Tracer.Debug($"The Task '{executor.GetType()}' Executing.");
                await executor.ExecuteAsync(econtext);
                Tracer.Debug($"The Task '{executor.GetType()}' Completed.");
            }
        }
    }

    /// <summary>
    /// 异步的 <see cref="IJob"/> 包装器。
    /// </summary>
    public class AsyncJobWrapper<TJob> : IJob where TJob : IAsyncTaskExecutor
    {
        /// <summary>
        /// 执行任务。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.MergedJobDataMap["serviceProvider"] as IServiceProvider;
            var arguments = context.MergedJobDataMap["arguments"] as IDictionary<string, object>;
            if (typeof(TJob).New(serviceProvider) is IAsyncTaskExecutor executor)
            {
                var initializer = context.MergedJobDataMap["initializer"] as Action<TJob>;
                initializer?.Invoke((TJob)executor);
                var econtext = new TaskExecuteContext(serviceProvider, arguments);
                Tracer.Debug($"The Task '{typeof(TJob)}' Executing.");
                await executor.ExecuteAsync(econtext);
                Tracer.Debug($"The Task '{typeof(TJob)}' Completed.");
            }
        }
    }
}
