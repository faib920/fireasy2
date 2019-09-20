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
                setting.UserName = configNode.GetAttributeValue("userName");
                setting.Password = configNode.GetAttributeValue("password");
                setting.ExchangeType = configNode.GetAttributeValue("exchangeType");
                setting.RequeueDelayTime = configNode.GetAttributeValue("requeueDelayTime").To<int?>();
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
                setting.UserName = configNode["userName"];
                setting.Password = configNode["password"];
                setting.ExchangeType = configNode["exchangeType"];
                setting.RequeueDelayTime = configNode["requeueDelayTime"].To<int?>();
            }

            return setting;
        }
#endif
    }
}
