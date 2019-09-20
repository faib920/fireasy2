// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
        private static readonly SafetyDictionary<string, IConfigurationSection> cfgCache = new SafetyDictionary<string, IConfigurationSection>();
        private static readonly SafetyDictionary<string, object> objCache = new SafetyDictionary<string, object>();

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
                return default(T);
            }

#if NETSTANDARD
            if (cfgCache.TryGetValue(attribute.Name, out IConfigurationSection value))
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

            return (T)cfgCache.GetOrAdd(attribute.Name, () =>
                {
                    var section = new T();
                    section.Bind(configuration.GetSection(attribute.Name.Replace("/", ":")));
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
                return cfgCache.GetOrAdd(sectionName, () => ConfigurationManager.GetSection(sectionName) as IConfigurationSection);
            }

            configFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);
            return cfgCache.GetOrAdd(sectionName, () => GetCustomConfiguration(sectionName, configFileName));
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

#if NETSTANDARD
        /// <summary>
        /// 绑定所有和 fireasy 有关的配置项。
        /// </summary>
        /// <param name="callAssembly"></param>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        public static void Bind(Assembly callAssembly, IConfiguration configuration, IServiceCollection services = null)
        {
            var assemblies = new List<Assembly>();

            FindReferenceAssemblies(callAssembly, assemblies);

            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType("Microsoft.Extensions.DependencyInjection.ConfigurationBinder");
                if (type != null)
                {
                    var method = type.GetMethod("Bind", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(IServiceCollection), typeof(IConfiguration) }, null);
                    if (method != null)
                    {
                        method.Invoke(null, new object[] { services, configuration });
                    }
                }
            }

            assemblies.Clear();
        }

        private static bool ExcludeAssembly(string assemblyName)
        {
            return !assemblyName.StartsWith("system.", StringComparison.OrdinalIgnoreCase) &&
                    !assemblyName.StartsWith("microsoft.", StringComparison.OrdinalIgnoreCase);
        }

        private static Assembly LoadAssembly(AssemblyName assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch
            {
                return null;
            }
        }

        private static void FindReferenceAssemblies(Assembly assembly, List<Assembly> assemblies)
        {
            foreach (var asb in assembly.GetReferencedAssemblies()
                .Where(s => ExcludeAssembly(s.Name))
                .Select(s => LoadAssembly(s))
                .Where(s => s != null))
            {
                if (!assemblies.Contains(asb))
                {
                    assemblies.Add(asb);
                }

                FindReferenceAssemblies(asb, assemblies);
            }
        }

#endif
        /// <summary>
        /// 根据提供的配置创建实例对象。
        /// </summary>
        /// <typeparam name="TSetting"></typeparam>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="setting"></param>
        /// <param name="typeFunc"></param>
        /// <returns></returns>
        public static TInstance CreateInstance<TSetting, TInstance>(IConfigurationSettingItem setting, Func<TSetting, Type> typeFunc) where TSetting : class, IConfigurationSettingItem
        {
            var relSetting = setting as TSetting;
            IConfigurationSettingItem extendSetting = null;
            if (setting is ExtendConfigurationSetting wsetting)
            {
                relSetting = wsetting.Base as TSetting;
                extendSetting = wsetting.Extend;
            }

            if (relSetting == null || typeFunc(relSetting) == null)
            {
                return default(TInstance);
            }

            var instance = typeFunc(relSetting).New<TInstance>();
            if (instance == null)
            {
                return default(TInstance);
            }

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
        /// <param name="factory"></param>
        /// <returns></returns>
        public static TInstance Cached<TInstance>(string cacheKey, Func<object> factory)
        {
            var obj = objCache.GetOrAdd(cacheKey, factory);
            if (obj != null)
            {
                return (TInstance)obj;
            }

            return default;
        }
    }
}
