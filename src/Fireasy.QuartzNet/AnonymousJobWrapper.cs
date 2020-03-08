// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Fireasy.QuartzNet
{
    /// <summary>
    /// 匿名的 <see cref="IJob"/> 包装器。
    /// </summary>
    public class AnonymousJobWrapper : IJob
    {
        /// <summary>
        /// 执行任务。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            if (context.MergedJobDataMap["executor"] is Action<IServiceProvider> executor)
            {
                var serviceProvider = context.MergedJobDataMap["serviceProvider"] as IServiceProvider;
                Tracer.Debug($"The Task '{executor.GetType()}' Executing.");
                executor(serviceProvider);
                Tracer.Debug($"The Task '{executor.GetType()}' Completed.");
            }
        }
    }
}
