using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
#if NETSTANDARD2_0
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
                setting.UserName = configNode.GetAttributeValue("userName");
                setting.Password = configNode.GetAttributeValue("password");
            }

            return setting;
        }

#if NETSTANDARD2_0
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
            }

            return setting;
        }
#endif
    }
}
