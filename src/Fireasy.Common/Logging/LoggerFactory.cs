// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Logging.Configuration;

namespace Fireasy.Common.Logging
{
    /// <summary>
    /// 日志管理器工厂。
    /// </summary>
    public static class LoggerFactory
    {
        /// <summary>
        /// 根据应用程序配置，创建日志管理器。
        /// </summary>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>缺省时，如果应用程序未配置，则为 <see cref="DefaultLogger"/>，否则为配置项对应的 <see cref="ILogger"/> 实例。</returns>
        public static ILogger CreateLogger(string configName = null)
        {
            ILogger logger;
            LoggingConfigurationSetting setting = null;
            var section = ConfigurationUnity.GetSection<LoggingConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                logger = section.Factory.CreateInstance(configName) as ILogger;
                if (logger != null)
                {
                    return logger;
                }
            }

            if (string.IsNullOrEmpty(configName))
            {
                if (section == null || section.Default == null)
                {
                    return DefaultLogger.Instance;
                }
                else
                {
                    setting = section.Default;
                }

            }
            else if (section != null)
            {
                setting = section.Settings[configName];
            }

            if (setting == null || setting.LogType == null)
            {
                return null;
            }

            return CreateBySetting(setting);
        }

        /// <summary>
        /// 根据提供的配置创建 <see cref="ILogger"/> 对象。
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        private static ILogger CreateBySetting(LoggingConfigurationSetting setting)
        {
            var caching = setting.LogType.New<ILogger>();
            if (caching == null)
            {
                return null;
            }

            caching.As<IConfigurationSettingHostService>(s => ConfigurationUnity.AttachSetting(s, setting));

            return caching;
        }
    }
}
