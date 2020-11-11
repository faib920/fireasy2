// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Tasks;
using Fireasy.Common.Threading;
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
        public Task Execute(IJobExecutionContext context)
        {
            if (context.MergedJobDataMap["executor"] is ITaskExecutor executor)
            {
                var serviceProvider = context.MergedJobDataMap["serviceProvider"] as IServiceProvider;
                var arguments = context.MergedJobDataMap["arguments"] as IDictionary<string, object>;

                using var scope = serviceProvider.TryCreateScope();
                var econtext = new TaskExecuteContext(scope.ServiceProvider, arguments, default);
                executor.Execute(econtext);
            }

            return TaskCompatible.CompletedTask;
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
        public Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.MergedJobDataMap["serviceProvider"] as IServiceProvider;
            var arguments = context.MergedJobDataMap["arguments"] as IDictionary<string, object>;

            if (typeof(TJob).New(serviceProvider) is ITaskExecutor executor)
            {
                var initializer = context.MergedJobDataMap["initializer"] as Action<TJob>;
                initializer?.Invoke((TJob)executor);
                using var scope = serviceProvider.TryCreateScope();
                var econtext = new TaskExecuteContext(scope.ServiceProvider, arguments, default);
                executor.Execute(econtext);
            }

            return TaskCompatible.CompletedTask;
        }
    }
}
