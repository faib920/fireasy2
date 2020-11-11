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
#endif
using System;

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 任务调度器的工厂。
    /// </summary>
    public static class TaskSchedulerFactory
    {
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
                services.AddSingleton(typeof(ITaskScheduler), sp => CreateScheduler(sp));
            }
            else
            {
                if (setting is ExtendConfigurationSetting extend)
                {
                    setting = extend.Base;
                }

                var tsset = (TaskScheduleConfigurationSetting)setting;
                services.AddSingleton(typeof(ITaskScheduler), sp => CreateScheduler(sp, tsset.Name));
            }

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IHostedService, ITaskScheduler>(sp => sp.GetService<ITaskScheduler>()));

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
            return CreateScheduler(null, configName);
        }

        /// <summary>
        /// 根据应用程序配置，创建任务调度管理器。
        /// </summary>
        /// <param name="serviceProvider">应用程序服务提供者实例。</param>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>缺省时，如果应用程序未配置，则为 <see cref="DefaultTaskScheduler"/>，否则为配置项对应的 <see cref="ITaskScheduler"/> 实例。</returns>
        private static ITaskScheduler CreateScheduler(IServiceProvider serviceProvider, string configName = null)
        {
            ITaskScheduler manager;
            IConfigurationSettingItem setting = null;
            var section = ConfigurationUnity.GetSection<TaskScheduleConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                manager = ConfigurationUnity.Cached<ITaskScheduler>($"TaskScheduler_{configName ?? "default"}", serviceProvider,
                    () => section.Factory.CreateInstance(serviceProvider, configName) as ITaskScheduler);

                if (manager != null)
                {
                    return manager;
                }
            }

            if (string.IsNullOrEmpty(configName))
            {
                if (section == null || (setting = section.GetDefault()) == null)
                {
                    return serviceProvider != null ?
                        new DefaultTaskScheduler(serviceProvider) : DefaultTaskScheduler.Instance;
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

            return ConfigurationUnity.Cached<ITaskScheduler>($"TaskScheduler_{configName ?? "default"}", serviceProvider,
                () => ConfigurationUnity.CreateInstance<TaskScheduleConfigurationSetting, ITaskScheduler>(serviceProvider, setting, s => s.SchedulerType));
        }
    }
}
