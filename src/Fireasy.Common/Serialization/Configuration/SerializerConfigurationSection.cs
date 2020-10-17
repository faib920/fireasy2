// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using System;
using System.Xml;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Common.Serialization.Configuration
{
    /// <summary>
    /// 提供对文本序列化器的配置管理。对应的配置节为 fireasy/serialzers(.net framework) 或 fireasy:serialzers(.net core)。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/serialzers")]
    public sealed class SerializerConfigurationSection : ManagableConfigurationSection<SerializerConfigurationSetting>
    {
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(
                section,
                "serializer",
                func: node => new SerializerConfigurationSetting
                {
                    Name = node.GetAttributeValue("name"),
                    SerializerType = Type.GetType(node.GetAttributeValue("type"), false, true)
                });

            //取默认实例
            DefaultInstanceName = section.GetAttributeValue("default");

            base.Initialize(section);
        }

#if NETSTANDARD
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="configuration">对应的配置节点。</param>
        public override void Bind(IConfiguration configuration)
        {
            Bind(configuration,
                "settings",
                func: c => new SerializerConfigurationSetting
                {
                    Name = c.Key,
                    SerializerType = Type.GetType(c.GetSection("type").Value, false, true)
                });

            //取默认实例
            DefaultInstanceName = configuration.GetSection("default").Value;

            base.Bind(configuration);
        }
#endif
    }
}
