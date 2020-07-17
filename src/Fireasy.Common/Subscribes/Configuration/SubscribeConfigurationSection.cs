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

namespace Fireasy.Common.Subscribes.Configuration
{
    /// <summary>
    /// 提供对订阅管理器的配置管理。对应的配置节为 fireasy/subscribers(.net framework) 或 fireasy:subscribers(.net core)。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/subscribers")]
    public sealed class SubscribeConfigurationSection : ManagableConfigurationSection<SubscribeConfigurationSetting>
    {
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(
                section,
                "subscriber",
                func: node => new SubscribeConfigurationSetting
                {
                    Name = node.GetAttributeValue("name"),
                    SubscriberType = Type.GetType(node.GetAttributeValue("type"), false, true)
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
                func: c => new SubscribeConfigurationSetting
                {
                    Name = c.Key,
                    SubscriberType = Type.GetType(c.GetSection("type").Value, false, true)
                });

            //取默认实例
            DefaultInstanceName = configuration.GetSection("default").Value;

            base.Bind(configuration);
        }
#endif
    }
}
