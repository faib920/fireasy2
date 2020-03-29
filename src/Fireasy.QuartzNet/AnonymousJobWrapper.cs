// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Quartz;
using System;
using System.Threading;
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
            if (context.MergedJobDataMap["executor"] is Action<IServiceProvider, CancellationToken> executor)
            {
                var serviceProvider = context.MergedJobDataMap["serviceProvider"] as IServiceProvider;
                var cancellationToken = (CancellationToken)context.MergedJobDataMap["cancellationToken"];

                executor(serviceProvider, cancellationToken);
            }
        }
    }
}
