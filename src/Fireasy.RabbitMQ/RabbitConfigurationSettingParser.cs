﻿// -----------------------------------------------------------------------
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

namespace Fireasy.RabbitMQ
{
    public class RabbitConfigurationSettingParser : IConfigurationSettingParseHandler
    {
        public IConfigurationSettingItem Parse(XmlNode section)
        {
            var setting = new RabbitConfigurationSetting();
            var configNode = section.SelectSingleNode("config");
            if (configNode != null)
            {
                var serializerType = configNode.GetAttributeValue("serializerType");
                if (!string.IsNullOrEmpty(serializerType))
                {
                    setting.SerializerType = serializerType.ParseType();
                }

                setting.Server = configNode.GetAttributeValue("server");
                setting.Port = configNode.GetAttributeValue<int>("port", -1);
                setting.UserName = configNode.GetAttributeValue("userName");
                setting.Password = configNode.GetAttributeValue("password");
                setting.ExchangeType = configNode.GetAttributeValue("exchangeType");
                setting.VirtualHost = configNode.GetAttributeValue("virtualHost");
                setting.RetryDelayTime = configNode.GetAttributeValue("retryDelayTime").ToTimeSpan(TimeSpan.FromSeconds(20));
                setting.RetryTimes = configNode.GetAttributeValue("retryTimes").To<int?>();
            }

            return setting;
        }

#if NETSTANDARD
        public IConfigurationSettingItem Parse(IConfiguration configuration)
        {
            var setting = new RabbitConfigurationSetting();
            var configNode = configuration.GetSection("config");
            if (configNode.Exists())
            {
                var serializerType = configNode["serializerType"];
                if (!string.IsNullOrEmpty(serializerType))
                {
                    setting.SerializerType = serializerType.ParseType();
                }

                setting.Server = configNode["server"];
                setting.Port = configNode["port"].To<int>(-1);
                setting.UserName = configNode["userName"];
                setting.Password = configNode["password"];
                setting.ExchangeType = configNode["exchangeType"];
                setting.VirtualHost = configNode["virtualHost"];
                setting.RetryDelayTime = configNode["retryDelayTime"].ToTimeSpan(TimeSpan.FromSeconds(20));
                setting.RetryTimes = configNode["retryTimes"].To<int?>();
            }

            return setting;
        }
#endif
    }
}
