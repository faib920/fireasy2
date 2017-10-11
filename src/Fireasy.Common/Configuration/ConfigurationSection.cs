// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Xml;
using Fireasy.Common.Extensions;
using System.Collections;

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
    }

    /// <summary>
    /// 一个抽象类，表示配置节的信息。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ConfigurationSection<T> : ConfigurationSection where T : IConfigurationSettingItem
    {
        /// <summary>
        /// 初始化 <see cref="ConfigurationSection&lt;T&gt;"/> 类的新实例。
        /// </summary>
        protected ConfigurationSection()
        {
            Settings = new ConfigurationSettings<T>();
        }

        /// <summary>
        /// 解析配置节下的所有子节点。
        /// </summary>
        /// <param name="section">当前的配置节点。</param>
        /// <param name="nodeName">要枚举的子节点的名称。</param>
        /// <param name="typeNodeName">如果配置类中存在 <see cref="Type"/> 的属性，则指定该属性的名称。</param>
        /// <param name="func">用于初始化设置项的函数。</param>
        protected void InitializeNode(XmlNode section, string nodeName, string typeNodeName = "type", Func<XmlNode, T> func = null)
        {
            section.EachChildren(
                nodeName, node =>
                {
                    var name = node.GetAttributeValue("name");
                    if (string.IsNullOrEmpty(name))
                    {
                        name = "setting" + Settings.Count;
                    }

                    try
                    {
                        var setting = default(T);
                        if (!string.IsNullOrEmpty(typeNodeName))
                        {
                            var typeName = node.GetAttributeValue(typeNodeName);
                            if (!string.IsNullOrEmpty(typeName))
                            {
                                var type = Type.GetType(typeName, false, true);

                                setting = ParseSetting(node, type);
                                if (setting == null && func != null)
                                {
                                    setting = func(node);
                                }
                            }
                        }

                        if (setting == null && func != null)
                        {
                            setting = func(node);
                        }

                        if (setting != null)
                        {
                            Settings.Add(name, setting);
                        }
                    }
                    catch (Exception ex)
                    {
                        Settings.AddInvalidSetting(name, ex);
                    }
                });
        }

        /// <summary>
        /// 返回当前节的配置项集合。
        /// </summary>
        public ConfigurationSettings<T> Settings { get; private set; }

        /// <summary>
        /// 根据是否忽略配置节处理接口来进行构造。
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        private T GetSettingIgnore(XmlNode node, Type type, Func<XmlNode, T> func)
        {
            if (func != null && type.IsDefined<ConfigurationSettingIgnoreAttribute>())
            {
                return func(node);
            }

            return default(T);
        }

        private T ParseSetting(XmlNode node, Type type)
        {
            var att = type.GetCustomAttributes<ConfigurationSettingAttribute>().FirstOrDefault();
            if (att != null)
            {
                var att1 = att.Type.GetCustomAttributes<ConfigurationSettingParseTypeAttribute>().FirstOrDefault();
                if (att1 == null)
                {
                    return att.Type.New<T>();
                }
                else
                {
                    var handler = att1.HandlerType.New<IConfigurationSettingParseHandler>();
                    if (handler != null)
                    {
                        return (T)handler.Parse(node);
                    }
                }
            }

            return default(T);
        }
    }

    /// <summary>
    /// 基于实例的配置节。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class InstanceConfigurationSection<T> : ConfigurationSection<T> where T : IConfigurationSettingItem
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
    }
}
