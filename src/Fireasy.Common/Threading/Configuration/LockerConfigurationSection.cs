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

namespace Fireasy.Common.Threading.Configuration
{
    /// <summary>
    /// 提供对锁的配置管理。对应的配置节为 fireasy/lockers(.net framework) 或 fireasy:lockers(.net core)。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/lockers")]
    public sealed class LockerConfigurationSection : ManagableConfigurationSection<LockerConfigurationSetting>
    {
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(
                section,
                "locker",
                func: node => new LockerConfigurationSetting
                {
                    Name = node.GetAttributeValue("name"),
                    LockerType = Type.GetType(node.GetAttributeValue("type"), false, true)
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
                func: c => new LockerConfigurationSetting
                {
                    Name = c.Key,
                    LockerType = Type.GetType(c.GetSection("type").Value, false, true)
                });

            //取默认实例
            DefaultInstanceName = configuration.GetSection("default").Value;

            base.Bind(configuration);
        }
#endif
    }
}
