// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching.Configuration;
using Fireasy.Common.Configuration;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif
using System;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 缓存管理器的工厂。
    /// </summary>
    public static class CacheManagerFactory
    {
#if NETSTANDARD
        public static IServiceCollection AddCaching(this IServiceCollection services)
        {
            var section = ConfigurationUnity.GetSection<CachingConfigurationSection>();
            if (section == null)
            {
                return services;
            }

            var setting = section.GetDefault();

            if (setting == null)
            {
                services.AddSingleton(typeof(ICacheManager), sp => CreateManager(sp));
            }
            else
            {
                if (setting is ExtendConfigurationSetting extend)
                {
                    setting = extend.Base;
                }

                services.AddSingleton(typeof(ICacheManager), sp => CreateManager(sp, ((CachingConfigurationSetting)setting).Name));
            }

            services.AddSingleton<IMemoryCacheManager>(sp => new MemoryCacheManager(sp))
                .AddSingleton(sp => sp.GetService(typeof(ICacheManager)) as IDistributedCacheManager);

            return services;
        }
#endif
        /// <summary>
        /// 根据应用程序配置，创建缓存管理器。
        /// </summary>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>缺省时，如果应用程序未配置，则为 <see cref="MemoryCacheManager"/>，否则为配置项对应的 <see cref="ICacheManager"/> 实例。</returns>
        public static ICacheManager CreateManager(string configName = null)
        {
            return CreateManager(null, configName);
        }

        /// <summary>
        /// 根据应用程序配置，创建缓存管理器。
        /// </summary>
        /// <param name="serviceProvider">应用程序服务提供者实例。</param>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>缺省时，如果应用程序未配置，则为 <see cref="MemoryCacheManager"/>，否则为配置项对应的 <see cref="ICacheManager"/> 实例。</returns>
        private static ICacheManager CreateManager(IServiceProvider serviceProvider, string configName = null)
        {
            ICacheManager manager;
            IConfigurationSettingItem setting = null;
            var section = ConfigurationUnity.GetSection<CachingConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                manager = ConfigurationUnity.Cached<ICacheManager>($"CacheManager_{configName}", serviceProvider,
                    () => section.Factory.CreateInstance(serviceProvider, configName) as ICacheManager);
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
                        new MemoryCacheManager(serviceProvider) : MemoryCacheManager.Instance;
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

            return ConfigurationUnity.Cached<ICacheManager>($"CacheManager_{configName}", serviceProvider,
                () => ConfigurationUnity.CreateInstance<CachingConfigurationSetting, ICacheManager>(serviceProvider, setting, s => s.CacheType));
        }
    }
}
