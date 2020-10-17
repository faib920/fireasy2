﻿// -----------------------------------------------------------------------
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
    /// 同步的 <see cref="IJob"/> 包装器。
    /// </summary>
    public class SyncJobWrapper : IJob
    {
        /// <summary>
        /// 执行任务。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            if (context.MergedJobDataMap["executor"] is ITaskExecutor executor)
            {
                var service = context.MergedJobDataMap["serviceProvider"] as IServiceProvider;
                var arguments = context.MergedJobDataMap["arguments"] as IDictionary<string, object>;
                var cancellationToken = (CancellationToken)context.MergedJobDataMap["cancellationToken"];

                var econtext = new TaskExecuteContext(service, arguments, cancellationToken);
                executor.Execute(econtext);
            }
        }
    }

    /// <summary>
    /// 同步的 <see cref="IJob"/> 包装器。
    /// </summary>
    public class SyncJobWrapper<TJob> : IJob where TJob : ITaskExecutor
    {
        /// <summary>
        /// 执行任务。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var service = context.MergedJobDataMap["serviceProvider"] as IServiceProvider;
            var arguments = context.MergedJobDataMap["arguments"] as IDictionary<string, object>;
            var cancellationToken = (CancellationToken)context.MergedJobDataMap["cancellationToken"];

            if (typeof(TJob).New(service) is ITaskExecutor executor)
            {
                var initializer = context.MergedJobDataMap["initializer"] as Action<TJob>;
                initializer?.Invoke((TJob)executor);
                var econtext = new TaskExecuteContext(service, arguments, cancellationToken);
                executor.Execute(econtext);
            }
        }
    }
}
