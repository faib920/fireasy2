﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Logging.Configuration;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif
using System;

namespace Fireasy.Common.Logging
{
    /// <summary>
    /// 日志管理器工厂。
    /// </summary>
    public static class LoggerFactory
    {
#if NETSTANDARD
        internal static IServiceCollection AddLogger(this IServiceCollection services)
        {
            var section = ConfigurationUnity.GetSection<LoggingConfigurationSection>();
            if (section == null)
            {
                return services;
            }

            var setting = section.GetDefault();

            if (setting == null)
            {
                services.AddTransient(typeof(ILogger), sp => CreateLogger(sp));
            }
            else
            {
                if (setting is ExtendConfigurationSetting extend)
                {
                    setting = extend.Base;
                }

                services.AddTransient(typeof(ILogger), sp => CreateLogger(sp, ((LoggingConfigurationSetting)setting).Name));
            }

            return services;
        }
#endif
        /// <summary>
        /// 根据应用程序配置，创建日志管理器。
        /// </summary>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>缺省时，如果应用程序未配置，则为 <see cref="DefaultLogger"/>，否则为配置项对应的 <see cref="ILogger"/> 实例。</returns>
        public static ILogger CreateLogger(string configName = null)
        {
            return CreateLogger(null, configName);
        }

        /// <summary>
        /// 根据应用程序配置，创建日志管理器。
        /// </summary>
        /// <param name="serviceProvider">应用程序服务提供者实例。</param>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>缺省时，如果应用程序未配置，则为 <see cref="DefaultLogger"/>，否则为配置项对应的 <see cref="ILogger"/> 实例。</returns>
        private static ILogger CreateLogger(IServiceProvider serviceProvider, string configName = null)
        {
            ILogger logger;
            IConfigurationSettingItem setting = null;
            var section = ConfigurationUnity.GetSection<LoggingConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                logger = ConfigurationUnity.Cached<ILogger>($"Logger_{configName}", serviceProvider,
                    () => section.Factory.CreateInstance(serviceProvider, configName) as ILogger);

                if (logger != null)
                {
                    return logger;
                }
            }

            if (string.IsNullOrEmpty(configName))
            {
                if (section == null || (setting = section.GetDefault()) == null)
                {
                    return DefaultLogger.Instance;
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

            return ConfigurationUnity.Cached<ILogger>($"Logger_{configName}", serviceProvider,
                () => ConfigurationUnity.CreateInstance<LoggingConfigurationSetting, ILogger>(serviceProvider, setting, s => s.LogType));
        }
    }
}
