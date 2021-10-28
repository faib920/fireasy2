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
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Fireasy.Redis
{
    /// <summary>
    /// Redis 配置的解析处理器。
    /// </summary>
    public class RedisConfigurationSettingParser : IConfigurationSettingParseHandler
    {
        public IConfigurationSettingItem Parse(XmlNode section)
        {
            var setting = new RedisConfigurationSetting();
            setting.ConnectionString = section.GetAttributeValue("connectionString");
            var configNode = section.SelectSingleNode("config");
            if (configNode != null)
            {
                var serializerType = configNode.GetAttributeValue("serializerType");
                if (!string.IsNullOrEmpty(serializerType))
                {
                    setting.SerializerType = serializerType.ParseType();
                }

                setting.DefaultDb = configNode.GetAttributeValue("defaultDb", 0);
                setting.DbRange = configNode.GetAttributeValue("dbRange");
                setting.KeyRule = configNode.GetAttributeValue("keyRule");
                setting.Password = configNode.GetAttributeValue("password");
                setting.Ssl = configNode.GetAttributeValue<bool>("ssl");
                setting.WriteBuffer = configNode.GetAttributeValue<int?>("writeBuffer");
                setting.LockTimeout = configNode.GetAttributeValue("lockTimeout").ToTimeSpan(TimeSpan.FromSeconds(10));
                setting.ConnectTimeout = configNode.GetAttributeValue("connectTimeout").ToTimeSpan(TimeSpan.FromMilliseconds(5000));
                setting.SyncTimeout = configNode.GetAttributeValue("syncTimeout").ToTimeSpan(TimeSpan.FromMilliseconds(10000));
                setting.RetryDelayTime = configNode.GetAttributeValue("retryDelayTime").ToTimeSpan(TimeSpan.FromSeconds(20));
                setting.RetryTimes = configNode.GetAttributeValue("retryTimes").To<int?>();
                setting.SlidingTime = configNode.GetAttributeValue("slidingTime").ToTimeSpan();
                setting.IgnoreException = configNode.GetAttributeValue("ignoreException").To(true);
                setting.Prefix = configNode.GetAttributeValue<string>("prefix");
                setting.Twemproxy = configNode.GetAttributeValue<bool?>("twemproxy");

                ReadHosts("hosts", configNode, setting.Hosts);
            }

            return setting;
        }

        private void ReadHosts(string name, XmlNode configNode, List<RedisHost> hosts)
        {
            if (!string.IsNullOrEmpty(configNode.SelectSingleNode(name)?.InnerText))
            {
                hosts.AddRange(configNode.SelectSingleNode(name)?.InnerText.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => new RedisHost(s)));
            }
            else
            {
                foreach (XmlNode nd in configNode.SelectNodes(name.Left(name.Length - 1)))
                {
                    RedisHost host;
                    if (!string.IsNullOrEmpty(nd.InnerText))
                    {
                        host = new RedisHost(nd.InnerText);
                    }
                    else
                    {
                        host = new RedisHost(nd.GetAttributeValue("server"), nd.GetAttributeValue("port", 0))
                        {
                            ReadOnly = nd.GetAttributeValue("readonly", false)
                        };
                    }

                    hosts.Add(host);
                }
            }
        }

#if NETSTANDARD
        public IConfigurationSettingItem Parse(IConfiguration configuration)
        {
            var setting = new RedisConfigurationSetting();
            setting.ConnectionString = configuration["connectionString"];
            var configNode = configuration.GetSection("config");
            if (configNode.Exists())
            {
                var serializerType = configNode["serializerType"];
                if (!string.IsNullOrEmpty(serializerType))
                {
                    setting.SerializerType = serializerType.ParseType();
                }

                setting.DefaultDb = configNode["defaultDb"].To(0);
                setting.DbRange = configNode["dbRange"];
                setting.KeyRule = configNode["keyRule"];
                setting.Password = configNode["password"];
                setting.Ssl = configNode["ssl"].To<bool>();
                setting.WriteBuffer = configNode["writeBuffer"].To<int?>();
                setting.LockTimeout = configNode["lockTimeout"].ToTimeSpan(TimeSpan.FromSeconds(10));
                setting.ConnectTimeout = configNode["connectTimeout"].ToTimeSpan(TimeSpan.FromMilliseconds(5000));
                setting.SyncTimeout = configNode["syncTimeout"].ToTimeSpan(TimeSpan.FromMilliseconds(10000));
                setting.RetryDelayTime = configNode["retryDelayTime"].ToTimeSpan(TimeSpan.FromSeconds(20));
                setting.RetryTimes = configNode["retryTimes"].To<int?>();
                setting.SlidingTime = configNode["slidingTime"].ToTimeSpan();
                setting.IgnoreException = configNode["ignoreException"].To(true);
                setting.Prefix = configNode["prefix"];
                setting.Twemproxy = configNode["twemproxy"].To<bool?>();

                ReadHosts("hosts", configNode, setting.Hosts);
            }

            return setting;
        }

        private void ReadHosts(string name, IConfiguration configNode, List<RedisHost> hosts)
        {
            foreach (var nd in configNode.GetSection(name).GetChildren())
            {
                RedisHost host;
                if (!string.IsNullOrEmpty(nd.Value))
                {
                    host = new RedisHost(nd.Value);
                }
                else
                {
                    host = new RedisHost(nd["server"], nd["port"].To(0))
                    {
                        ReadOnly = nd["readonly"].To(false)
                    };
                }

                hosts.Add(host);
            }
        }

#endif
    }
}
