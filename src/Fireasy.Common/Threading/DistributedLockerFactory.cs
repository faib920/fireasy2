// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Threading.Configuration;
using System;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Fireasy.Common.Threading
{
    /// <summary>
    /// 分布式锁的工厂。
    /// </summary>
    public static class DistributedLockerFactory
    {
#if NETSTANDARD
        internal static IServiceCollection AddLocker(this IServiceCollection services)
        {
            var section = ConfigurationUnity.GetSection<LockerConfigurationSection>();
            if (section == null)
            {
                return services;
            }

            var setting = section.GetDefault();

            if (setting == null)
            {
                return services;
            }
            else
            {
                if (setting is ExtendConfigurationSetting extend)
                {
                    setting = extend.Base;
                }

                services.AddSingleton(typeof(IDistributedLocker), sp => CreateLocker(sp, ((LockerConfigurationSetting)setting).Name));
            }

            return services;
        }
#endif

        /// <summary>
        /// 根据应用程序配置，创建一个分布式锁。
        /// </summary>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>为配置项对应的 <see cref="IDistributedLocker"/> 实例。</returns>
        public static IDistributedLocker CreateLocker(string configName = null)
        {
            return CreateLocker(null, configName);
        }

        /// <summary>
        /// 根据应用程序配置，创建一个分布式锁。
        /// </summary>
        /// <param name="serviceProvider">应用程序服务提供者实例。</param>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>为配置项对应的 <see cref="IDistributedLocker"/> 实例。</returns>
        private static IDistributedLocker CreateLocker(IServiceProvider serviceProvider, string configName = null)
        {
            IDistributedLocker locker;
            IConfigurationSettingItem setting = null;
            var section = ConfigurationUnity.GetSection<LockerConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                locker = ConfigurationUnity.Cached<IDistributedLocker>($"Locker_{configName}", serviceProvider,
                    () => section.Factory.CreateInstance(serviceProvider, configName) as IDistributedLocker);
                if (locker != null)
                {
                    return locker;
                }
            }

            if (string.IsNullOrEmpty(configName))
            {
                if (section == null || (setting = section.GetDefault()) == null)
                {
                    return null;
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

            return ConfigurationUnity.Cached<IDistributedLocker>($"Locker_{configName}", serviceProvider,
                () => ConfigurationUnity.CreateInstance<LockerConfigurationSetting, IDistributedLocker>(serviceProvider, setting, s => s.LockerType));
        }
    }
}
