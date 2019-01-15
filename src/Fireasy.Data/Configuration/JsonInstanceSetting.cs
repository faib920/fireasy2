// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
using Microsoft.CSharp;
#endif

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 一个提供数据库字符串配置的类，使用Json文件进行配置。
    /// </summary>
    [Serializable]
    public sealed class JsonInstanceSetting : DefaultInstanceConfigurationSetting
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
                return Parse(fileName);
            }

#if NETSTANDARD
            public IConfigurationSettingItem Parse(IConfiguration configuration)
            {
                var fileName = configuration.GetSection("fileName").Value;
                return Parse(fileName);
            }
#endif

            private IConfigurationSettingItem Parse(string fileName)
            {
                fileName = DbUtility.ResolveFullPath(fileName);
                if (!File.Exists(fileName))
                {
                    throw new FileNotFoundException(SR.GetString(SRKind.FileNotFound, fileName), fileName);
                }

                var content = File.ReadAllText(fileName);
                var dict = (IDictionary<string, object>)new JsonSerializer().Deserialize<object>(content);
                var setting = new JsonInstanceSetting();

                setting.FileName = fileName;
                setting.ProviderName = dict.TryGetValue("providerName", () => string.Empty)?.ToString();
                setting.ProviderType = dict.TryGetValue("providerType", () => string.Empty)?.ToString();

                if (dict.TryGetValue("databaseType", out object databaseType))
                {
                    setting.DatabaseType = Type.GetType((string)databaseType);
                }

                if (dict.TryGetValue("connectionString", out object connectionString))
                {
                    setting.ConnectionString = ConnectionStringHelper.GetConnectionString((string)connectionString);
                }

                return setting;
            }
        }
    }
}
