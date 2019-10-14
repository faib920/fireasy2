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
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Common.Caching.Configuration
{
    /// <summary>
    /// 提供对缓存管理器的配置管理。对应的配置节为 fireasy/cachings。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/cachings")]
    public sealed class CachingConfigurationSection : ManagableConfigurationSection<CachingConfigurationSetting>
    {
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(
                section, 
                "caching", 
                func: node => new CachingConfigurationSetting
                {
                        Name = node.GetAttributeValue("name"),
                        CacheType = Type.GetType(node.GetAttributeValue("type"), false, true)
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
                func: c => new CachingConfigurationSetting
                    {
                        Name = c.Key,
                        CacheType = Type.GetType(c.GetSection("type").Value, false, true)
                    });

            //取默认实例
            DefaultInstanceName = configuration.GetSection("default").Value;

            base.Bind(configuration);
        }
#endif
    }
}
