// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#else
using System.Configuration;
using System.IO;
using System.Xml;
#endif

namespace Fireasy.Common.Configuration
{
    /// <summary>
    /// 应用程序配置的管理单元。
    /// </summary>
    public static class ConfigurationUnity
    {
        private const string CUSTOM_CONFIG_NAME = "my-config-file";
        private static readonly SafetyDictionary<string, IConfigurationSection> _cfgCache = new SafetyDictionary<string, IConfigurationSection>();
        private static readonly SafetyDictionary<string, object> _objCache = new SafetyDictionary<string, object>();

        /// <summary>
        /// 获取配置节实例。
        /// </summary>
        /// <typeparam name="T">配置节的类型。</typeparam>
        /// <returns></returns>
        public static T GetSection<T>() where T : IConfigurationSection
        {
            var attribute = typeof(T).GetCustomAttributes<ConfigurationSectionStorageAttribute>().FirstOrDefault();
            if (attribute == null)
            {
                return default;
            }

#if NETSTANDARD
            if (_cfgCache.TryGetValue(attribute.Name, out IConfigurationSection value))
            {
                return (T)value;
            }

            return default;
#else
            return (T)GetSection(attribute.Name);
#endif
        }

        /// <summary>
        /// 为具有 <see cref="IConfigurationSettingHostService"/> 接口的对象附加相应的配置对象。
        /// </summary>
        /// <param name="hostService"></param>
        /// <param name="setting"></param>
        public static void AttachSetting(IConfigurationSettingHostService hostService, IConfigurationSettingItem setting)
        {
            if (hostService != null)
            {
                hostService.Attach(setting);
            }
        }

#if NETSTANDARD
        /// <summary>
        /// 将配置信息绑定到 <typeparamref name="T"/> 对象内部。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static T Bind<T>(IConfiguration configuration) where T : IConfigurationSection, new()
        {
            var attribute = typeof(T).GetCustomAttributes<ConfigurationSectionStorageAttribute>().FirstOrDefault();
            if (attribute == null)
            {
                return default;
            }

            return (T)_cfgCache.GetOrAdd(attribute.Name, key =>
                {
                    var section = new T();
                    var bindConfiguration = new BindingConfiguration(configuration, key.Replace("/", ":"));
                    section.Bind(bindConfiguration);

                    if (section is IConfigurationSectionWithCount wc)
                    {
                        Tracer.Debug($"The {typeof(T).Name} was bound ({wc.Count} items).");
                    }
                    else
                    {
                        Tracer.Debug($"The {typeof(T).Name} was bound.");
                    }

                    return section;
                });
        }
#endif

#if !NETSTANDARD
        private static IConfigurationSection GetSection(string sectionName)
        {
            //使用appSetting名称为FireasyConfigFileName放置自定义配置文件
            var configFileName = ConfigurationManager.AppSettings[CUSTOM_CONFIG_NAME];
            if (string.IsNullOrEmpty(configFileName))
            {
                return _cfgCache.GetOrAdd(sectionName, key => ConfigurationManager.GetSection(key) as IConfigurationSection);
            }

            configFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);
            return _cfgCache.GetOrAdd(sectionName, () => GetCustomConfiguration(sectionName, configFileName));
        }

        /// <summary>
        /// 从自定义配置文件中读取相应的配置。
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="configFileName"></param>
        /// <returns></returns>
        private static IConfigurationSection GetCustomConfiguration(string sectionName, string configFileName)
        {
            var config = ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap { ExeConfigFilename = configFileName },
                ConfigurationUserLevel.None);
            var section = config.GetSection(sectionName);
            return ReadSection(sectionName, section);
        }

        private static IConfigurationSection ReadSection(string sectionName, System.Configuration.ConfigurationSection section)
        {
            if (section == null)
            {
                return null;
            }

            var handlerType = Type.GetType(section.SectionInformation.Type, false);
            if (handlerType == null)
            {
                throw new ConfigurationErrorsException(SR.GetString(SRKind.UnableReadConfiguration, sectionName),
                    new TypeLoadException(section.SectionInformation.Type, null));
            }

            var handler = handlerType.New<IConfigurationSectionHandler>();
            var doc = new XmlDocument();
            var xml = section.SectionInformation.GetRawXml();
            try
            {
                doc.LoadXml(xml);
                return handler.Create(null, null, doc.ChildNodes[0]) as IConfigurationSection;
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(SR.GetString(SRKind.UnableReadConfiguration, sectionName), ex);
            }
        }
#endif

        /// <summary>
        /// 根据提供的配置创建实例对象。
        /// </summary>
        /// <typeparam name="TSetting"></typeparam>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <param name="setting"></param>
        /// <param name="typeGetter"></param>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static TInstance CreateInstance<TSetting, TInstance>(IServiceProvider serviceProvider, IConfigurationSettingItem setting, Func<TSetting, Type> typeGetter, Action<TSetting, TInstance> initializer = null) where TSetting : class, IConfigurationSettingItem
        {
            var relSetting = setting as TSetting;
            IConfigurationSettingItem extendSetting = null;
            if (setting is ExtendConfigurationSetting wsetting)
            {
                relSetting = wsetting.Base as TSetting;
                extendSetting = wsetting.Extend;
            }

            Type type;
            if (relSetting == null || (type = typeGetter(relSetting)) == null)
            {
                return default;
            }

            var instance = serviceProvider != null ?
                (TInstance)type.New(serviceProvider) : type.New<TInstance>().TryUseContainer();

            if (instance == null)
            {
                return default;
            }

            initializer?.Invoke(relSetting, instance);

            if (extendSetting != null)
            {
                instance.As<IConfigurationSettingHostService>(s => AttachSetting(s, extendSetting));
            }

            return instance;
        }

        /// <summary>
        /// 缓存配置项创建的实例。
        /// </summary>
        /// <typeparam name="TSetting"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="valueCreator"></param>
        /// <returns></returns>
        public static TInstance Cached<TInstance>(string cacheKey, IServiceProvider serviceProvider, Func<object> valueCreator)
        {
            if (serviceProvider != null)
            {
                return (TInstance)valueCreator();
            }

            var obj = _objCache.GetOrAdd(cacheKey, valueCreator);
            if (obj != null)
            {
                return (TInstance)obj;
            }

            return default;
        }
    }
}
