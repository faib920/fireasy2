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

namespace Fireasy.Common.Localization.Configuration
{
    /// <summary>
    /// 提供对字符串本地化管理器的配置管理。对应的配置节为 fireasy/stringLocalizers(.net framework) 或 fireasy:stringLocalizers(.net core)。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/stringLocalizers")]
    public sealed class StringLocalizerConfigurationSection : ManagableConfigurationSection<StringLocalizerConfigurationSetting>
    {
        /// <summary>
        /// 获取或设置当前使用的区域。
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(
                section,
                "localizer",
                func: node => new StringLocalizerConfigurationSetting
                {
                    Name = node.GetAttributeValue("name"),
                    LocalizerType = Type.GetType(node.GetAttributeValue("type"), false, true),
                });

            //取默认实例
            DefaultInstanceName = section.GetAttributeValue("default");
            Culture = section.GetAttributeValue("culture");

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
                func: c => new StringLocalizerConfigurationSetting
                {
                    Name = c.Key,
                    LocalizerType = Type.GetType(c.GetSection("type").Value, false, true),
                });

            //取默认实例
            DefaultInstanceName = configuration.GetSection("default").Value;
            Culture = configuration.GetSection("culture").Value;

            base.Bind(configuration);
        }
#endif
    }
}
