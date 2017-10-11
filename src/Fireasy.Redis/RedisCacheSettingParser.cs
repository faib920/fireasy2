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

namespace Fireasy.Redis
{
    /// <summary>
    /// Redis 配置的解析处理器。
    /// </summary>
    internal class RedisCacheSettingParser : IConfigurationSettingParseHandler
    {
        IConfigurationSettingItem IConfigurationSettingParseHandler.Parse(System.Xml.XmlNode section)
        {
            var setting = new RedisCacheSetting();
            setting.Name = section.GetAttributeValue("name");
            setting.CacheType = Type.GetType(section.GetAttributeValue("type"), false, true);
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

                foreach (XmlNode nd in configNode.SelectNodes("host"))
                {
                    var host = new RedisCacheHost();
                    host.Server = nd.GetAttributeValue("server");
                    host.Port = nd.GetAttributeValue("port", 0);
                    host.ReadOnly = nd.GetAttributeValue("readonly", false);

                    setting.Hosts.Add(host);
                }
            }

            return setting;
        }
    }
}
