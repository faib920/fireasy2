// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Xml;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 数据库实例配置类。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/dataInstances")]
    public sealed class InstanceConfigurationSection : ConfigurationSection<IInstanceConfigurationSetting>
    {
        private string defaultInstanceName;

        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(
                section,
                "instance",
                null,
                node => CreateDataInstanceSetting(node));

            //取默认实例
            defaultInstanceName = section.GetAttributeValue("default");
        }

        /// <summary>
        /// 获取默认的配置实例。
        /// </summary>
        public IInstanceConfigurationSetting Default
        {
            get { return string.IsNullOrEmpty(defaultInstanceName) ? Settings["setting0"] : Settings[defaultInstanceName]; }
        }

        /// <summary>
        /// 根据实例名创建相应的配置实例。
        /// </summary>
        /// <param name="node">Section节点。</param>
        /// <returns>返回相应类型的配置实例。</returns>
        private static IInstanceConfigurationSetting CreateDataInstanceSetting(XmlNode node)
        {
            var handerType = Type.GetType(node.GetAttributeValue("handlerType"), false, true);
            var handler = handerType != null ? handerType.New<IConfigurationSettingParseHandler>() :
                InstanceParseHandleFactory.GetParseHandler(node.GetAttributeValue("storeType"));

            if (handler != null)
            {
                return handler.Parse(node) as IInstanceConfigurationSetting;
            }
            return null;
        }
    }
}
