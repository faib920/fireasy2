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
using System.Collections.Generic;
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
    public sealed class StringInstanceSetting : DefaultInstanceConfigurationSetting
    {
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
                var clusters = node.SelectSingleNode("clusters");
                var key = node.GetAttributeValue("key");

                if (clusters != null)
                {
                    return Parse(storeType, providerName, providerType, databaseType, key, clusters);
                }

                var connectionString = !string.IsNullOrEmpty(node.InnerText) ? node.InnerText : node.GetAttributeValue("connectionString");

                return Parse(storeType, providerName, providerType, databaseType, key, connectionString);
            }

#if NETSTANDARD2_0
            public IConfigurationSettingItem Parse(IConfiguration configuration)
            {
                var storeType = configuration.GetSection("storeType").Value;
                var providerName = configuration.GetSection("providerName").Value;
                var providerType = configuration.GetSection("providerType").Value;
                var databaseType = configuration.GetSection("databaseType").Value;
                var clusters = configuration.GetSection("clusters");
                var key = configuration.GetSection("key").Value;

                if (clusters.Exists())
                {
                    return Parse(storeType, providerName, providerType, databaseType, key, clusters);
                }

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

            private IConfigurationSettingItem Parse(string storeType, string providerName, string providerType, string databaseType, string key, XmlNode clusters)
            {
                var setting = new StringInstanceSetting();

                setting.ProviderName = providerName;
                setting.ProviderType = providerType;
                setting.DatabaseType = string.IsNullOrEmpty(databaseType) ? null : Type.GetType(databaseType, false, true);

                var master = clusters.SelectSingleNode("master");
                var slaves = clusters.SelectSingleNode("slaves");

                if (master != null)
                {
                    var connstr = master.GetAttributeValue("connectionString");
                    var cluster = new ClusteredConnectionSetting
                        {
                            ConnectionString = ConnectionStringHelper.GetConnectionString(connstr),
                            Mode = DistributedMode.Master
                        };
                    setting.Clusters.Add(cluster);
                }

                if (slaves != null)
                {
                    foreach (XmlNode node in slaves.ChildNodes)
                    {
                        var connstr = node.GetAttributeValue("connectionString");
                        var cluster = new ClusteredConnectionSetting
                            {
                                ConnectionString = ConnectionStringHelper.GetConnectionString(connstr),
                                Mode = DistributedMode.Slave,
                                Weight = node.GetAttributeValue("weight", 0)
                            };
                        setting.Clusters.Add(cluster);
                    }
                }

                return setting;
            }

#if NETSTANDARD2_0
            private IConfigurationSettingItem Parse(string storeType, string providerName, string providerType, string databaseType, string key, IConfiguration clusters)
            {
                var setting = new StringInstanceSetting();

                setting.ProviderName = providerName;
                setting.ProviderType = providerType;
                setting.DatabaseType = string.IsNullOrEmpty(databaseType) ? null : Type.GetType(databaseType, false, true);

                var master = clusters.GetSection("master");
                var slaves = clusters.GetSection("slaves");

                if (master.Exists())
                {
                    var connstr = master["connectionString"];
                    var cluster = new ClusteredConnectionSetting
                        {
                            ConnectionString = ConnectionStringHelper.GetConnectionString(connstr),
                            Mode = DistributedMode.Master
                        };
                    setting.Clusters.Add(cluster);
                }

                if (slaves.Exists())
                {
                    foreach (var child in slaves.GetChildren())
                    {
                        var connstr = child["connectionString"];
                        var cluster = new ClusteredConnectionSetting
                            {
                                ConnectionString = ConnectionStringHelper.GetConnectionString(connstr),
                                Mode = DistributedMode.Slave,
                                Weight = child["weight"].To(0)
                            };
                        setting.Clusters.Add(cluster);
                    }
                }

                return setting;
            }
#endif
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
