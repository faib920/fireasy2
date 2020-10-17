// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Tasks;
using Fireasy.QuartzNet;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 Quartz 任务调度管理组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzScheduler(this IServiceCollection services, Action<QuartzScheduleOptions> setupAction = null)
        {
            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            services.AddSingleton<ITaskScheduler, TaskScheduler>();

            services.TryAddEnumerable(
                    ServiceDescriptor.Singleton<IHostedService, ITaskScheduler>(p => p.GetService<ITaskScheduler>()));

            return services;
        }
    }
}
#endif