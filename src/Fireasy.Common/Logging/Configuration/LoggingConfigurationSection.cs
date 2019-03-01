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

namespace Fireasy.Common.Logging.Configuration
{
    /// <summary>
    /// 表示日志的配置节。无法继承此类。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/loggings")]
    public sealed class LoggingConfigurationSection : ManagableConfigurationSection<LoggingConfigurationSetting>
    {
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
            DefaultInstanceName = section.GetAttributeValue("default");
            Level = GetLevel(section.GetAttributeValue("level"));

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
                func: c => new LoggingConfigurationSetting
                    {
                        Name = c.Key,
                        LogType = Type.GetType(c.GetSection("type").Value, false, true)
                    });

            DefaultInstanceName = configuration.GetSection("default").Value;
            Level = GetLevel(configuration.GetSection("level").Value);

            base.Bind(configuration);
        }
#endif

        /// <summary>
        /// 获取或设置日志级别。
        /// </summary>
        public LogLevel Level { get; set; }

        private LogLevel GetLevel(string levels)
        {
            var level = LogLevel.Default;

            if (string.IsNullOrEmpty(levels))
            {
                return level;
            }

            foreach (var segment in levels.Split(new[] { '|', ';', ',' }))
            {
                if (segment.Equals("info", StringComparison.InvariantCultureIgnoreCase))
                {
                    level |= LogLevel.Info;
                }
                else if (segment.Equals("error", StringComparison.InvariantCultureIgnoreCase))
                {
                    level |= LogLevel.Error;
                }
                else if (segment.Equals("debug", StringComparison.InvariantCultureIgnoreCase))
                {
                    level |= LogLevel.Debug;
                }
                else if (segment.Equals("warn", StringComparison.InvariantCultureIgnoreCase))
                {
                    level |= LogLevel.Warn;
                }
                else if (segment.Equals("warn", StringComparison.InvariantCultureIgnoreCase))
                {
                    level |= LogLevel.Fatal;
                }
            }

            return level;
        }
    }
}
