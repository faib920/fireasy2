// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Configuration;
using System.Xml;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Data.Provider;

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 一个提供数据库字符串配置的类，使用配置文件中的字符串进行配置。
    /// </summary>
    [Serializable]
    public sealed class StringInstanceSetting : IInstanceConfigurationSetting
    {
        /// <summary>
        /// 返回提供者配置名称。
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// 获取实例名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 返回数据库类型。
        /// </summary>
        public string ProviderType { get; set; }

        /// <summary>
        /// 返回数据库类型。
        /// </summary>
        public Type DatabaseType { get; set; }

        /// <summary>
        /// 返回数据库连接字符串。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 返回字符串的引用类型。
        /// </summary>
        public StringReferenceType ReferenceType { get; set; }

        internal class SettingParseHandler : IConfigurationSettingParseHandler
        {
            public IConfigurationSettingItem Parse(XmlNode node)
            {
                var setting = new StringInstanceSetting();
                var storeType = node.GetAttributeValue("storeType");

                setting.Name = node.GetAttributeValue("name");
                setting.ProviderName = node.GetAttributeValue("providerName");
                setting.ProviderType = node.GetAttributeValue("providerType");
                setting.DatabaseType = Type.GetType(node.GetAttributeValue("databaseType"), false, true);
                var key = node.GetAttributeValue("key");

                switch (storeType.ToLower())
                {
                    case "appsettings":
                        setting.ReferenceType = StringReferenceType.AppSettings;
                        if (!string.IsNullOrEmpty(key))
                        {
                            setting.ConnectionString = ConnectionStringHelper.GetConnectionString(ConfigurationManager.AppSettings[key]);
                        }
                        break;
                    case "connectionstrings":
                        setting.ReferenceType = StringReferenceType.ConnectionStrings;
                        if (!string.IsNullOrEmpty(key))
                        {
                            var connstr = ConfigurationManager.ConnectionStrings[key];
                            if (connstr != null)
                            {
                                setting.ConnectionString = ConnectionStringHelper.GetConnectionString(connstr.ConnectionString);
                            }
                        }
                        break;
                    default:
                        setting.ReferenceType = StringReferenceType.String;
                        setting.ConnectionString = ConnectionStringHelper.GetConnectionString(node.GetAttributeValue("connectionString"));
                        break;
                }
                return setting;
            }
        }
    }

    /// <summary>
    /// 字符串引用类别。
    /// </summary>
    public enum StringReferenceType
    {
        /// <summary>
        /// 直接给定的字符串。
        /// </summary>
        String,
        /// <summary>
        /// 使用appSettings配置节。
        /// </summary>
        AppSettings,
        /// <summary>
        /// 使用connectionStrings配置节。
        /// </summary>
        ConnectionStrings
    }
}
