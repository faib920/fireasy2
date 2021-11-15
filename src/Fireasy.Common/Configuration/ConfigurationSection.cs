// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Fireasy.Common.Configuration
{
    /// <summary>
    /// 一个抽象类，表示配置节的信息。
    /// </summary>
    public abstract class ConfigurationSection : IConfigurationSection
    {
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public virtual void Initialize(XmlNode section)
        {
        }


#if NETSTANDARD
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="configuration">对应的配置节点。</param>
        public virtual void Bind(IConfiguration configuration)
        {

        }
#endif
    }

    /// <summary>
    /// 一个抽象类，表示配置节的信息。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ConfigurationSection<T> : ConfigurationSection, IConfigurationSectionWithCount where T : IConfigurationSettingItem
    {
        private readonly ConfigurationSettings<IConfigurationSettingItem> innerSettings = new ConfigurationSettings<IConfigurationSettingItem>();
        private ConfigurationSettings<T> settings;
        private static readonly object locker = new object();

        /// <summary>
        /// 解析配置节下的所有子节点。
        /// </summary>
        /// <param name="section">当前的配置节点。</param>
        /// <param name="nodeName">要枚举的子节点的名称。</param>
        /// <param name="typeNodeName">如果配置类中存在 <see cref="Type"/> 的属性，则指定该属性的名称。</param>
        /// <param name="func">用于初始化设置项的函数。</param>
        protected void InitializeNode(XmlNode section, string nodeName, string typeNodeName = "type", Func<XmlNode, IConfigurationSettingItem> func = null)
        {
            section.EachChildren(
                nodeName, node =>
                {
                    var name = node.GetAttributeValue("name");
                    if (string.IsNullOrEmpty(name))
                    {
                        name = string.Concat("setting", innerSettings.Count);
                    }

                    try
                    {
                        var setting = func(node);
                        if (!string.IsNullOrEmpty(typeNodeName))
                        {
                            var typeName = node.GetAttributeValue(typeNodeName);
                            if (!string.IsNullOrEmpty(typeName))
                            {
                                var type = Type.GetType(typeName, true, true);

                                var extend = ParseSetting(node, type);
                                if (extend != null)
                                {
                                    setting = new ExtendConfigurationSetting { Base = setting, Extend = extend };
                                }
                            }
                        }

                        if (setting is INamedIConfigurationSettingItem named)
                        {
                            named.Name = name;
                        }

                        if (setting != null)
                        {
                            innerSettings.Add(name, setting);
                        }
                    }
                    catch (Exception ex)
                    {
                        Tracer.Error($"Read configuration section of '{nodeName}-{name}' throw exception:{ex.Output()}");
                        innerSettings.AddInvalidSetting(name, ex);
                    }
                });
        }

#if NETSTANDARD
        /// <summary>
        /// 解析配置节下的所有子节点。
        /// </summary>
        /// <param name="configuration">当前的配置节点。</param>
        /// <param name="nodeName">要枚举的子节点的名称。</param>
        /// <param name="typeNodeName">如果配置类中存在 <see cref="Type"/> 的属性，则指定该属性的名称。</param>
        /// <param name="func">用于初始化设置项的函数。</param>
        protected void Bind(IConfiguration configuration, string nodeName, string typeNodeName = "type", Func<Microsoft.Extensions.Configuration.IConfigurationSection, IConfigurationSettingItem> func = null)
        {
            foreach (var child in configuration.GetSection(nodeName).GetChildren())
            {
                var bindConfiguration = new BindingConfiguration(configuration, child);
                var name = child.Key;
                if (string.IsNullOrEmpty(name))
                {
                    name = string.Concat("setting", innerSettings.Count);
                }

                try
                {
                    var setting = func(bindConfiguration);
                    if (!string.IsNullOrEmpty(typeNodeName))
                    {
                        var typeName = child.GetSection(typeNodeName).Value;
                        if (!string.IsNullOrEmpty(typeName))
                        {
                            var type = Type.GetType(typeName, true, true);

                            var extend = ParseSetting(bindConfiguration, type);
                            if (extend != null)
                            {
                                setting = new ExtendConfigurationSetting { Base = setting, Extend = extend };
                            }
                        }
                    }

                    if (setting is INamedIConfigurationSettingItem named)
                    {
                        named.Name = name;
                    }

                    if (setting != null)
                    {
                        innerSettings.Add(name, setting);
                    }
                }
                catch (Exception ex)
                {
                    Tracer.Error($"Read configuration section of '{nodeName}-{name}' throw exception:{ex.Output()}");
                    innerSettings.AddInvalidSetting(name, ex);
                }
            }
        }
#endif

        /// <summary>
        /// 返回当前节的配置项集合。
        /// </summary>
        public ConfigurationSettings<T> Settings
        {
            get
            {
                if (settings == null)
                {
                    lock (locker)
                    {
                        if (settings != null)
                        {
                            return settings;
                        }

                        settings = new ConfigurationSettings<T>();

                        foreach (var kvp in innerSettings)
                        {
                            if (kvp.Value is ExtendConfigurationSetting extend)
                            {
                                settings.Add(kvp.Key, (T)extend.Base);
                            }
                            else
                            {
                                settings.Add(kvp.Key, (T)kvp.Value);
                            }
                        }
                    }
                }

                return settings;
            }
        }

        int IConfigurationSectionWithCount.Count
        {
            get
            {
                return Settings.Count;
            }
        }

        public IEnumerable<IConfigurationSettingItem> GetSettings()
        {
            return innerSettings.Values;
        }

        public IConfigurationSettingItem GetSetting(string name)
        {
            if (innerSettings.ContainsKey(name))
            {
                return innerSettings[name];
            }

            return null;
        }

        private IConfigurationSettingItem ParseSetting(XmlNode node, Type type)
        {
            var att = type.GetCustomAttributes<ConfigurationSettingAttribute>(true).FirstOrDefault();
            if (att != null)
            {
                var att1 = att.Type.GetCustomAttributes<ConfigurationSettingParseTypeAttribute>().FirstOrDefault();
                if (att1 == null)
                {
                    return att.Type.New<IConfigurationSettingItem>();
                }
                else
                {
                    var handler = att1.HandlerType.New<IConfigurationSettingParseHandler>();
                    if (handler != null)
                    {
                        return handler.Parse(node);
                    }
                }
            }

            return null;
        }

#if NETSTANDARD
        private IConfigurationSettingItem ParseSetting(IConfiguration configuration, Type type)
        {
            var att = type.GetCustomAttributes<ConfigurationSettingAttribute>(true).FirstOrDefault();
            if (att != null)
            {
                var att1 = att.Type.GetCustomAttributes<ConfigurationSettingParseTypeAttribute>().FirstOrDefault();
                if (att1 == null)
                {
                    return att.Type.New<IConfigurationSettingItem>();
                }
                else
                {
                    var handler = att1.HandlerType.New<IConfigurationSettingParseHandler>();
                    if (handler != null)
                    {
                        return handler.Parse(configuration);
                    }
                }
            }

            return null;
        }
#endif

    }

    /// <summary>
    /// 具有默认实例配置的配置节。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DefaultInstaneConfigurationSection<T> : ConfigurationSection<T> where T : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置默认配置实例名称。
        /// </summary>
        public string DefaultInstanceName { get; set; }

        public IConfigurationSettingItem GetDefault()
        {
            if (Settings.Count == 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(DefaultInstanceName))
            {
                if (Settings.ContainsKey("setting0"))
                {
                    return GetSetting("setting0");
                }

                return GetSettings().FirstOrDefault();
            }

            return GetSetting(DefaultInstanceName);
        }

        /// <summary>
        /// 获取默认的配置项。
        /// </summary>
        public T Default
        {
            get
            {
                var setting = GetDefault();
                if (setting == null)
                {
                    return default;
                }

                if (setting is ExtendConfigurationSetting extend)
                {
                    return (T)extend.Base;
                }

                return (T)setting;
            }
        }
    }

    /// <summary>
    /// 可托管的基于实例的配置节。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ManagableConfigurationSection<T> : DefaultInstaneConfigurationSection<T> where T : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取实例创建工厂。
        /// </summary>
        public IManagedFactory Factory { get; private set; }

        public override void Initialize(XmlNode section)
        {
            var factory = section.GetAttributeValue("managed");
            if (!string.IsNullOrEmpty(factory))
            {
                var type = factory.ParseType();
                if (type != null)
                {
                    Factory = type.New<IManagedFactory>();
                }
            }

            base.Initialize(section);
        }

#if NETSTANDARD
        public override void Bind(IConfiguration configuration)
        {
            var factory = configuration.GetSection("managed").Value;
            if (!string.IsNullOrEmpty(factory))
            {
                var type = factory.ParseType();
                if (type != null)
                {
                    Factory = type.New<IManagedFactory>();
                }
            }

            base.Bind(configuration);
        }
#endif
    }
}
