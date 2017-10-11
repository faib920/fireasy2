// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using Fireasy.Common.Caching.Configuration;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 缓存管理器的工厂。
    /// </summary>
    public static class CacheManagerFactory
    {
        /// <summary>
        /// 根据应用程序配置，创建缓存管理器。
        /// </summary>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>缺省时，如果应用程序未配置，则为 <see cref="MemoryCacheManager"/>，否则为配置项对应的 <see cref="ICacheManager"/> 实例。</returns>
        public static ICacheManager CreateManager(string configName = null)
        {
            ICacheManager manager;
            CachingConfigurationSetting setting = null;
            var section = ConfigurationUnity.GetSection<CachingConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                manager = section.Factory.CreateInstance(configName) as ICacheManager;
                if (manager != null)
                {
                    return manager;
                }
            }

            if (string.IsNullOrEmpty(configName))
            {
                if (section == null || section.Default == null)
                {
                    return MemoryCacheManager.Instance;
                }

                setting = section.Default;
            }
            else if (section != null)
            {
                setting = section.Settings[configName];
            }

            if (setting == null || setting.CacheType == null)
            {
                return null;
            }

            return CreateBySetting(setting);
        }

        /// <summary>
        /// 根据提供的配置创建 <see cref="ICacheManager"/> 对象。
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        private static ICacheManager CreateBySetting(CachingConfigurationSetting setting)
        {
            var caching = setting.CacheType.New<ICacheManager>();
            if (caching == null)
            {
                return null;
            }

            caching.As<IConfigurationSettingHostService>(s => ConfigurationUnity.AttachSetting(s, setting));

            return caching;
        }
    }
}
