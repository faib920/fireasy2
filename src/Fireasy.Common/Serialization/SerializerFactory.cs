// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Serialization.Configuration;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif
using System;

namespace Fireasy.Common.Serialization
{
    public static class SerializerFactory
    {
#if NETSTANDARD
        public static IServiceCollection AddSerializer(this IServiceCollection services)
        {
            var section = ConfigurationUnity.GetSection<SerializerConfigurationSection>();
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

                services.AddSingleton(typeof(ISerializer), sp => CreateSerializer(sp, ((SerializerConfigurationSetting)setting).Name));
            }

            return services;
        }
#endif
        /// <summary>
        /// 根据应用程序配置，创建文本序列化器。
        /// </summary>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns></returns>
        public static ISerializer CreateSerializer(string configName = null)
        {
            return CreateSerializer(null, configName);
        }

        /// <summary>
        /// 根据应用程序配置，创建文本序列化器。
        /// </summary>
        /// <param name="serviceProvider">应用程序服务提供者实例。</param>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns></returns>
        private static ISerializer CreateSerializer(IServiceProvider serviceProvider, string configName = null)
        {
            ISerializer serializer;
            IConfigurationSettingItem setting = null;
            var section = ConfigurationUnity.GetSection<SerializerConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                serializer = ConfigurationUnity.Cached<ISerializer>($"Serializer_{configName}", serviceProvider,
                    () => section.Factory.CreateInstance(serviceProvider, configName) as ISerializer);

                if (serializer != null)
                {
                    return serializer;
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

            return ConfigurationUnity.Cached<ISerializer>($"Serializer_{configName}", serviceProvider,
                () => ConfigurationUnity.CreateInstance<SerializerConfigurationSetting, ISerializer>(serviceProvider, setting, s => s.SerializerType));
        }
    }
}
