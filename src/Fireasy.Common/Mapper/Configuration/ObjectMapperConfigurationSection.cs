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

namespace Fireasy.Common.Mapper.Configuration
{
    /// <summary>
    /// 提供对对象映射的配置管理。对应的配置节为 fireasy/objMappers(.net framework) 或 fireasy:objMappers(.net core)。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/objMappers")]
    public sealed class ObjectMapperConfigurationSection : ManagableConfigurationSection<ObjectMapperConfigurationSetting>
    {
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(
                section,
                "mapper",
                func: node => new ObjectMapperConfigurationSetting
                {
                    Name = node.GetAttributeValue("name"),
                    MapperType = Type.GetType(node.GetAttributeValue("type"), false, true)
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
                func: c => new ObjectMapperConfigurationSetting
                {
                    Name = c.Key,
                    MapperType = Type.GetType(c.GetSection("type").Value, false, true)
                });

            //取默认实例
            DefaultInstanceName = configuration.GetSection("default").Value;

            base.Bind(configuration);
        }
#endif
    }
}
