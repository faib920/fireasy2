// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Tasks.Configuration;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
#endif
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 任务调度器的工厂。
    /// </summary>
    public static class TaskSchedulerFactory
    {
        private static readonly MethodInfo StartMethod = typeof(ITaskScheduler).GetMethod(nameof(ITaskScheduler.Start));
        private static readonly MethodInfo StartAsyncMethod = typeof(ITaskScheduler).GetMethod(nameof(ITaskScheduler.StartAsync));

#if NETSTANDARD
        public static IServiceCollection AddTaskScheduler(this IServiceCollection services)
        {
            var section = ConfigurationUnity.GetSection<TaskScheduleConfigurationSection>();
            if (section == null)
            {
                return services;
            }

            var setting = section.GetDefault();

            if (setting == null)
            {
                services.AddSingleton(typeof(ITaskScheduler), DefaultTaskScheduler.Instance);
            }
            else
            {
                if (setting is ExtendConfigurationSetting extend)
                {
                    setting = extend.Base;
                }

                var scset = (TaskScheduleConfigurationSetting)setting;
                var schedulerType = scset.SchedulerType;
                services.AddSingleton(typeof(ITaskScheduler), schedulerType);
                InitializeExecutors(scset, services);
                services.TryAddEnumerable(
                    ServiceDescriptor.Singleton<IHostedService, ITaskScheduler>(p =>
                        ConfigurationUnity.Cached<ITaskScheduler>($"TaskScheduler_{scset.Name}", () => p.GetService<ITaskScheduler>())));
            }

            return services;
        }
#endif

        /// <summary>
        /// 根据应用程序配置，创建任务调度管理器。
        /// </summary>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>缺省时，如果应用程序未配置，则为 <see cref="DefaultTaskScheduler"/>，否则为配置项对应的 <see cref="ITaskScheduler"/> 实例。</returns>
        public static ITaskScheduler CreateScheduler(string configName = null)
        {
            ITaskScheduler manager;
            IConfigurationSettingItem setting = null;
            var section = ConfigurationUnity.GetSection<TaskScheduleConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                manager = ConfigurationUnity.Cached<ITaskScheduler>($"TaskScheduler_{configName ?? "default"}", () => section.Factory.CreateInstance(configName) as ITaskScheduler);
                if (manager != null)
                {
                    return manager;
                }
            }

            if (string.IsNullOrEmpty(configName))
            {
                if (section == null || (setting = section.GetDefault()) == null)
                {
                    return DefaultTaskScheduler.Instance;
                }
            }
            else if (section != null)
            {
                setting = section.GetSetting(configName);
            }

            if (setting == null)
            {
                return null;
            }

            return ConfigurationUnity.Cached<ITaskScheduler>($"TaskScheduler_{configName ?? "default"}", () => ConfigurationUnity.CreateInstance<TaskScheduleConfigurationSetting, ITaskScheduler>(setting, s => s.SchedulerType, (s, t) => InitializeExecutors(s, t)));
        }

        private static ITaskScheduler InitializeExecutors(TaskScheduleConfigurationSetting setting, ITaskScheduler scheduler)
        {
            foreach (var exsetting in setting.ExecutorSettings)
            {
                if (exsetting.ExecutorType == null)
                {
                    continue;
                }

                if (typeof(IAsyncTaskExecutor).IsAssignableFrom(exsetting.ExecutorType))
                {
                    StartAsyncMethod.MakeGenericMethod(exsetting.ExecutorType).Invoke(scheduler, new object[] { exsetting.Delay, exsetting.Period, null });
                }
                else if (typeof(ITaskExecutor).IsAssignableFrom(exsetting.ExecutorType))
                {
                    StartMethod.MakeGenericMethod(exsetting.ExecutorType).Invoke(scheduler, new object[] { exsetting.Delay, exsetting.Period, null });
                }
            }

            return scheduler;
        }

#if NETSTANDARD
        private static void InitializeExecutors(TaskScheduleConfigurationSetting setting, IServiceCollection services)
        {
            foreach (var exsetting in setting.ExecutorSettings)
            {
                if (exsetting.ExecutorType == null)
                {
                    continue;
                }

                if (typeof(IAsyncTaskExecutor).IsAssignableFrom(exsetting.ExecutorType) ||
                    typeof(ITaskExecutor).IsAssignableFrom(exsetting.ExecutorType))
                {
                    services.AddSingleton(exsetting.ExecutorType);

                    var sd = ServiceDescriptorHelper.CreateSingleton(typeof(TaskExecutorDefiniton), typeof(TaskExecutorDefiniton<>).MakeGenericType(exsetting.ExecutorType), () =>
                    {
                        var parExp = Expression.Parameter(typeof(IServiceProvider), "p");
                        var cons = typeof(TaskExecutorDefiniton<>).MakeGenericType(exsetting.ExecutorType).GetConstructors()[0];
                        var newExp = Expression.New(cons, Expression.Constant(exsetting.Delay), Expression.Constant(exsetting.Period));
                        return Expression.Lambda(newExp, parExp).Compile();
                    });

                    services.TryAddEnumerable(sd);
                }
            }
        }
#endif
    }
}
