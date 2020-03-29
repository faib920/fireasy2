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

                setting.PoolSize = configNode.GetAttributeValue<int?>("poolSize");
                setting.DefaultDb = configNode.GetAttributeValue("defaultDb", 0);
                setting.DbRange = configNode.GetAttributeValue("dbRange");
                setting.KeyRule = configNode.GetAttributeValue("keyRule");
                setting.Password = configNode.GetAttributeValue("password");
                setting.Ssl = configNode.GetAttributeValue<bool>("ssl");
                setting.WriteBuffer = configNode.GetAttributeValue<int?>("writeBuffer");
                setting.LockTimeout = configNode.GetAttributeValue("lockTimeout").ToTimeSpan(TimeSpan.FromSeconds(10));
                setting.ConnectTimeout = configNode.GetAttributeValue("connectTimeout").ToTimeSpan(TimeSpan.FromMilliseconds(5000));
                setting.SyncTimeout = configNode.GetAttributeValue("syncTimeout").ToTimeSpan(TimeSpan.FromMilliseconds(10000));
                setting.RequeueDelayTime = configNode.GetAttributeValue("requeueDelayTime").ToTimeSpan();
                setting.SlidingTime = configNode.GetAttributeValue("slidingTime").ToTimeSpan();

                foreach (XmlNode nd in configNode.SelectNodes("host"))
                {
                    var host = new RedisHost(nd.GetAttributeValue("server"), nd.GetAttributeValue("port", 0))
                    {
                        ReadOnly = nd.GetAttributeValue("readonly", false)
                    };

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

                setting.PoolSize = configNode["poolSize"].To<int?>();
                setting.DefaultDb = configNode["defaultDb"].To(0);
                setting.DbRange = configNode["dbRange"];
                setting.KeyRule = configNode["keyRule"];
                setting.Password = configNode["password"];
                setting.Ssl = configNode["ssl"].To<bool>();
                setting.WriteBuffer = configNode["writeBuffer"].To<int?>();
                setting.LockTimeout = configNode["lockTimeout"].ToTimeSpan(TimeSpan.FromSeconds(10));
                setting.ConnectTimeout = configNode["connectTimeout"].ToTimeSpan(TimeSpan.FromMilliseconds(5000));
                setting.SyncTimeout = configNode["syncTimeout"].ToTimeSpan(TimeSpan.FromMilliseconds(10000));
                setting.RequeueDelayTime = configNode["requeueDelayTime"].ToTimeSpan();
                setting.SlidingTime = configNode["slidingTime"].ToTimeSpan();

                foreach (var nd in configNode.GetSection("host").GetChildren())
                {
                    var host = new RedisHost(nd["server"], nd["port"].To(0))
                    {
                        ReadOnly = nd["readonly"].To(false)
                    };

                    setting.Hosts.Add(host);
                }
            }

            return setting;
        }
#endif
    }
}
