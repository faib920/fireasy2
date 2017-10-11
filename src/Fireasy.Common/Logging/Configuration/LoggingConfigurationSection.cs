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

namespace Fireasy.Common.Logging.Configuration
{
    /// <summary>
    /// 表示日志的配置节。无法继承此类。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/loggings")]
    public sealed class LoggingConfigurationSection : InstanceConfigurationSection<LoggingConfigurationSetting>
    {
        private string defaultInstanceName;

        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(section, 
                "logging", 
                func: node => new LoggingConfigurationSetting
                    {
                        Name = node.GetAttributeValue("name"),
                        LogType = Type.GetType(node.GetAttributeValue("type"), false, true)
                    });

            //取默认实例
            defaultInstanceName = section.GetAttributeValue("default");

            base.Initialize(section);
        }

        /// <summary>
        /// 获取默认的日志配置。
        /// </summary>
        public LoggingConfigurationSetting Default
        {
            get { return string.IsNullOrEmpty(defaultInstanceName) ? Settings["setting0"] : Settings[defaultInstanceName]; }
        }

    }
}
