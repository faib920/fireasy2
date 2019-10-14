// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using Fireasy.Common.Threading.Configuration;
using Fireasy.Common.Configuration;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Fireasy.Common.Threading
{
    /// <summary>
    /// 分布式锁的工厂。
    /// </summary>
    public static class LockerFactory
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

                services.AddSingleton(typeof(IDistributedLocker), ((LockerConfigurationSetting)setting).LockerType);
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
            IDistributedLocker locker;
            IConfigurationSettingItem setting = null;
            var section = ConfigurationUnity.GetSection<LockerConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                locker = ConfigurationUnity.Cached<IDistributedLocker>($"Locker_{configName}", () => section.Factory.CreateInstance(configName) as IDistributedLocker);
                if (locker != null)
                {
                    return locker;
                }
            }

            if (string.IsNullOrEmpty(configName))
            {
                if (section == null || (setting = section.GetDefault()) == null)
                {
                    throw new NotSupportedException();
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

            return ConfigurationUnity.Cached<IDistributedLocker>($"Locker_{configName}", () => ConfigurationUnity.CreateInstance<LockerConfigurationSetting, IDistributedLocker>(setting, s => s.LockerType));
        }
    }
}
