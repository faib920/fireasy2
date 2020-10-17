// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Mapper.Configuration;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif
using System;

namespace Fireasy.Common.Mapper
{
    public static class ObjectMapperFactory
    {
#if NETSTANDARD
        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            var section = ConfigurationUnity.GetSection<ObjectMapperConfigurationSection>();
            if (section == null)
            {
                return services;
            }

            var setting = section.GetDefault();

            if (setting == null)
            {
                services.AddSingleton(typeof(IObjectMapper), sp => CreateMapper(sp));
            }
            else
            {
                if (setting is ExtendConfigurationSetting extend)
                {
                    setting = extend.Base;
                }

                services.AddSingleton(typeof(IObjectMapper), sp => CreateMapper(sp, ((ObjectMapperConfigurationSetting)setting).Name));
            }

            return services;
        }
#endif
        /// <summary>
        /// 根据应用程序配置，创建对象映射器。
        /// </summary>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns></returns>
        public static IObjectMapper CreateMapper(string configName = null)
        {
            return CreateMapper(null, configName);
        }

        /// <summary>
        /// 根据应用程序配置，创建对象映射器。
        /// </summary>
        /// <param name="serviceProvider">应用程序服务提供者实例。</param>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns></returns>
        private static IObjectMapper CreateMapper(IServiceProvider serviceProvider, string configName = null)
        {
            IObjectMapper mapper;
            IConfigurationSettingItem setting = null;
            var section = ConfigurationUnity.GetSection<ObjectMapperConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                mapper = ConfigurationUnity.Cached<IObjectMapper>($"Mapper_{configName}", serviceProvider,
                    () => section.Factory.CreateInstance(serviceProvider, configName) as IObjectMapper);

                if (mapper != null)
                {
                    return mapper;
                }
            }

            if (string.IsNullOrEmpty(configName))
            {
                if (section == null || (setting = section.GetDefault()) == null)
                {
                    return DefaultObjectMapper.Default;
                }
            }
            else if (section != null)
            {
                setting = section.GetSetting(configName);
            }

            if (setting == null)
            {
                return DefaultObjectMapper.Default;
            }

            return ConfigurationUnity.Cached<IObjectMapper>($"Mapper_{configName}", serviceProvider,
                () => ConfigurationUnity.CreateInstance<ObjectMapperConfigurationSetting, IObjectMapper>(serviceProvider, setting, s => s.MapperType));
        }
    }
}