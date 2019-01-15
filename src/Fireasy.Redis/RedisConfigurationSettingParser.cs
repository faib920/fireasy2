// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif
using System;
using System.Xml;

namespace Fireasy.Redis
{
    /// <summary>
    /// Redis 配置的解析处理器。
    /// </summary>
    public class RedisConfigurationSettingParser : IConfigurationSettingParseHandler
    {
        public IConfigurationSettingItem Parse(System.Xml.XmlNode section)
        {
            var setting = new RedisConfigurationSetting();
            setting.ConnectionString = section.GetAttributeValue("connectionString");
            setting.Twemproxy = section.GetAttributeValue<bool>("twemproxy");
            var configNode = section.SelectSingleNode("config");
            if (configNode != null)
            {
                var serializerType = configNode.GetAttributeValue("serializerType");
                if (!string.IsNullOrEmpty(serializerType))
                {
                    setting.SerializerType = serializerType.ParseType();
                }

                setting.MaxReadPoolSize = configNode.GetAttributeValue("maxReadPoolSize", 5);
                setting.MaxWritePoolSize = configNode.GetAttributeValue("maxWritePoolSize", 5);
                setting.DefaultDb = configNode.GetAttributeValue("defaultDb", 0);
                setting.Password = configNode.GetAttributeValue("password");
                setting.ConnectTimeout = configNode.GetAttributeValue<int?>("connectTimeout");
                setting.AdvanceDelay = configNode.GetAttributeValue<double?>("advanceDelay");

                foreach (XmlNode nd in configNode.SelectNodes("host"))
                {
                    var host = new RedisHost();
                    host.Server = nd.GetAttributeValue("server");
                    host.Port = nd.GetAttributeValue("port", 0);
                    host.ReadOnly = nd.GetAttributeValue("readonly", false);

                    setting.Hosts.Add(host);
                }
            }

            return setting;
        }

#if NETSTANDARD
        public IConfigurationSettingItem Parse(IConfiguration configuration)
        {
            var setting = new RedisConfigurationSetting();
            setting.ConnectionString = configuration["connectionString"];
            setting.Twemproxy = configuration["twemproxy"].To(false);
            var configNode = configuration.GetSection("config");
            if (configNode.Exists())
            {
                var serializerType = configNode["serializerType"];
                if (!string.IsNullOrEmpty(serializerType))
                {
                    setting.SerializerType = serializerType.ParseType();
                }

                setting.MaxReadPoolSize = configNode["maxReadPoolSize"].To(5);
                setting.MaxWritePoolSize = configNode["maxWritePoolSize"].To(5);
                setting.DefaultDb = configNode["defaultDb"].To(0);
                setting.Password = configNode["password"];
                setting.ConnectTimeout = configNode["connectTimeout"].To<int?>();
                setting.AdvanceDelay = configNode["advanceDelay"].To<double?>();

                foreach (var nd in configNode.GetSection("host").GetChildren())
                {
                    var host = new RedisHost();
                    host.Server = nd["server"];
                    host.Port = nd["port"].To(0);
                    host.ReadOnly = nd["readonly"].To(false);

                    setting.Hosts.Add(host);
                }
            }

            return setting;
        }
#endif
    }
}
