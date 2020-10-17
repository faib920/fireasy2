// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif
using System;
using System.Xml;

namespace Fireasy.Common.Logging.Configuration
{
    public class ComplexLoggingSettingParseHandler : IConfigurationSettingParseHandler
    {
        IConfigurationSettingItem IConfigurationSettingParseHandler.Parse(XmlNode section)
        {
            var setting = new ComplexLoggingSetting();
            setting.LogType = Type.GetType(section.Attributes["type"].InnerText, false, true);

            foreach (XmlNode node in section.SelectSingleNode("loggers").ChildNodes)
            {
                var level = LogEnvironment.GetLevel(node.Attributes["level"].InnerText);
                var logType = Type.GetType(node.Attributes["type"].InnerText, false, true);
                if (logType != null)
                {
                    setting.Pairs.Add(new ComplexLoggingSettingPair { Level = level, LogType = logType });
                }
            }

            return setting;
        }

#if NETSTANDARD
        IConfigurationSettingItem IConfigurationSettingParseHandler.Parse(IConfiguration configuration)
        {
            var setting = new ComplexLoggingSetting();
            setting.LogType = Type.GetType(configuration.GetSection("type").Value, false, true);

            foreach (var child in configuration.GetSection("loggers").GetChildren())
            {
                var level = LogEnvironment.GetLevel(child.GetSection("level").Value);
                var logType = Type.GetType(child.GetSection("type").Value, false, true);
                if (logType != null)
                {
                    setting.Pairs.Add(new ComplexLoggingSettingPair { Level = level, LogType = logType });
                }
            }

            return setting;
        }
#endif
    }
}
