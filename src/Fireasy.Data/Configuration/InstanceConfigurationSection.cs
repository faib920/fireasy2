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
using System.Linq;
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 数据库实例配置类。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/dataInstances")]
    public sealed class InstanceConfigurationSection : DefaultInstaneConfigurationSection<IInstanceConfigurationSetting>
    {
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
                CreateDataInstanceSetting);

            //取默认实例
            DefaultInstanceName = section.GetAttributeValue("default");
        }

#if NETSTANDARD2_0
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="configuration">对应的配置节点。</param>
        public override void Bind(IConfiguration configuration)
        {
            Bind(configuration, 
                "settings", 
                null,
                CreateDataInstanceSetting);

            DefaultInstanceName = configuration.GetSection("default").Value;

            base.Bind(configuration);
        }
#endif

        /// <summary>
        /// 根据实例名创建相应的配置实例。
        /// </summary>
        /// <param name="node">Section节点。</param>
        /// <returns>返回相应类型的配置实例。</returns>
        private static IInstanceConfigurationSetting CreateDataInstanceSetting(XmlNode node)
        {
            var typeName = node.GetAttributeValue("handlerType");
            var handerType = string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName, false, true);
            var handler = handerType != null ? handerType.New<IConfigurationSettingParseHandler>() :
                InstanceParseHandleFactory.GetParseHandler(node.GetAttributeValue("storeType"));

            if (handler != null)
            {
                return handler.Parse(node) as IInstanceConfigurationSetting;
            }
            return null;
        }

#if NETSTANDARD2_0
        /// <summary>
        /// 根据实例名创建相应的配置实例。
        /// </summary>
        /// <param name="configuration">Section节点。</param>
        /// <returns>返回相应类型的配置实例。</returns>
        private static IInstanceConfigurationSetting CreateDataInstanceSetting(IConfiguration configuration)
        {
            var typeName = configuration.GetSection("handlerType").Value;
            var handerType = string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName, false, true);
            var handler = handerType != null ? handerType.New<IConfigurationSettingParseHandler>() :
                InstanceParseHandleFactory.GetParseHandler(configuration.GetSection("storeType").Value);

            if (handler != null)
            {
                return handler.Parse(configuration) as IInstanceConfigurationSetting;
            }

            return null;
        }
#endif
    }
}
