// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading;
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
                var cancellationToken = (CancellationToken)context.MergedJobDataMap["cancellationToken"];

                var econtext = new TaskExecuteContext(service, arguments, cancellationToken);
                await executor.ExecuteAsync(econtext);
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
            var cancellationToken = (CancellationToken)context.MergedJobDataMap["cancellationToken"];

            if (typeof(TJob).New(serviceProvider) is IAsyncTaskExecutor executor)
            {
                var initializer = context.MergedJobDataMap["initializer"] as Action<TJob>;
                initializer?.Invoke((TJob)executor);
                var econtext = new TaskExecuteContext(serviceProvider, arguments, cancellationToken);
                await executor.ExecuteAsync(econtext);
            }
        }
    }
}
