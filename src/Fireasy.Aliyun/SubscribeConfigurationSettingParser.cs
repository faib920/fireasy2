using Fireasy.Common.Configuration;
using System.Xml;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Aliyun
{
    public class SubscribeConfigurationSettingParser : BaseAliyunConfigurationSettingParser
    {
        public override IConfigurationSettingItem Parse(XmlNode section)
        {
            var setting = new SubscribeConfigurationSetting();
            Parse(setting, section);

            return setting;
        }

#if NETSTANDARD
        public override IConfigurationSettingItem Parse(IConfiguration configuration)
        {
            var setting = new SubscribeConfigurationSetting();
            Parse(setting, configuration);

            var configNode = configuration.GetSection("config");
            if (configNode.Exists())
            {
                setting.InstanceId = configNode["instanceId"];
            }

            return setting;
        }
#endif
    }
}