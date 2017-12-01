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
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#else
using System.Configuration;
#endif

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
                var storeType = node.GetAttributeValue("storeType");
                var providerName = node.GetAttributeValue("providerName");
                var providerType = node.GetAttributeValue("providerType");
                var databaseType = node.GetAttributeValue("databaseType");
                var key = node.GetAttributeValue("key");
                var connectionString = node.GetAttributeValue("connectionString");

                return Parse(storeType, providerName, providerType, databaseType, key, connectionString);
            }

#if NETSTANDARD2_0
            public IConfigurationSettingItem Parse(IConfiguration configuration)
            {
                var storeType = configuration.GetSection("storeType").Value;
                var providerName = configuration.GetSection("providerName").Value;
                var providerType = configuration.GetSection("providerType").Value;
                var databaseType = configuration.GetSection("databaseType").Value;
                var key = configuration.GetSection("key").Value;
                var connectionString = configuration.GetSection("connectionString").Value;

                return Parse(storeType, providerName, providerType, databaseType, key, connectionString);
            }
#endif

            private IConfigurationSettingItem Parse(string storeType, string providerName, string providerType, string databaseType, string key, string connectionString)
            {
                var setting = new StringInstanceSetting();

                setting.ProviderName = providerName;
                setting.ProviderType = providerType;
                setting.DatabaseType = string.IsNullOrEmpty(databaseType) ? null : Type.GetType(databaseType, false, true);

                switch (string.Concat(string.Empty, storeType).ToLower())
                {
                    case "appsettings":
#if !NETSTANDARD2_0
                        setting.ReferenceType = StringReferenceType.AppSettings;
                        if (!string.IsNullOrEmpty(key))
                        {
                            setting.ConnectionString = ConnectionStringHelper.GetConnectionString(ConfigurationManager.AppSettings[key]);
                        }
#endif
                        break;
                    case "connectionstrings":
#if !NETSTANDARD2_0
                        setting.ReferenceType = StringReferenceType.ConnectionStrings;
                        if (!string.IsNullOrEmpty(key))
                        {
                            var connstr = ConfigurationManager.ConnectionStrings[key];
                            if (connstr != null)
                            {
                                setting.ConnectionString = ConnectionStringHelper.GetConnectionString(connstr.ConnectionString);
                            }
                        }
#endif
                        break;
                    default:
                        setting.ReferenceType = StringReferenceType.String;
                        setting.ConnectionString = ConnectionStringHelper.GetConnectionString(connectionString);
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
