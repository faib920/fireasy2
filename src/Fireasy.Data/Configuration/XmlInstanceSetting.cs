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
using System.IO;
using System.Xml;
using System.Xml.XPath;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 一个提供数据库字符串配置的类，使用XML文件进行配置。
    /// </summary>
    [Serializable]
    public sealed class XmlInstanceSetting : DefaultInstanceConfigurationSetting
    {
        /// <summary>
        /// 返回Xml文件名称。
        /// </summary>
        public string FileName { get; set; }

        internal class SettingParseHandler : IConfigurationSettingParseHandler
        {
            public IConfigurationSettingItem Parse(XmlNode node)
            {
                var fileName = node.GetAttributeValue("fileName");
                var xpath = node.GetAttributeValue("xmlPath");
                return Parse(fileName, xpath);
            }

#if NETSTANDARD
            public IConfigurationSettingItem Parse(IConfiguration configuration)
            {
                var fileName = configuration.GetSection("fileName").Value;
                var xpath = configuration.GetSection("xmlPath").Value;
                return Parse(fileName, xpath);
            }
#endif

            private IConfigurationSettingItem Parse(string fileName, string xpath)
            {
                fileName = DbUtility.ResolveFullPath(fileName);

                if (!File.Exists(fileName))
                {
                    throw new FileNotFoundException(SR.GetString(SRKind.FileNotFound, fileName), fileName);
                }

                var setting = new XmlInstanceSetting
                {
                    FileName = fileName
                };

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(fileName);
                XmlNode connNode = null;

                if (!string.IsNullOrEmpty(xpath))
                {
                    xmlDoc.SelectSingleNode(xpath);
                    if (connNode == null)
                    {
                        throw new XPathException(xpath);
                    }
                }
                else
                {
                    connNode = xmlDoc.DocumentElement;
                }

                setting.ProviderName = connNode.SelectSingleNode("providerName")?.InnerText ?? connNode.GetAttributeValue("providerName");
                setting.ProviderType = connNode.SelectSingleNode("providerType")?.InnerText ?? connNode.GetAttributeValue("providerType");

                var databaseType = connNode.SelectSingleNode("databaseType")?.InnerText ?? connNode.GetAttributeValue("databaseType");
                if (!string.IsNullOrEmpty(databaseType))
                {
                    setting.DatabaseType = Type.GetType(databaseType, false, true);
                }

                setting.ConnectionString = ConnectionStringHelper.GetConnectionString(connNode.SelectSingleNode("connectionString")?.InnerText ?? connNode.GetAttributeValue("connectionString"));

                return setting;
            }
        }
    }
}
