using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif
using System.Xml;

namespace Fireasy.Aliyun
{
    public abstract class BaseAliyunConfigurationSettingParser : IConfigurationSettingParseHandler
    {
        public abstract IConfigurationSettingItem Parse(XmlNode section);

        protected void Parse(BaseAliyunConfigurationSetting setting, XmlNode section)
        {
            var configNode = section.SelectSingleNode("config");
            if (configNode != null)
            {
                setting.AccessId = configNode.GetAttributeValue("accessId");
                setting.AccessSecret = configNode.GetAttributeValue("accessSecret");
                setting.RegionId = configNode.GetAttributeValue("regionId");
            }
        }

#if NETSTANDARD
        public abstract IConfigurationSettingItem Parse(IConfiguration configuration);

        protected void Parse(BaseAliyunConfigurationSetting setting, IConfiguration configuration)
        {
            var configNode = configuration.GetSection("config");
            if (configNode.Exists())
            {
                setting.AccessId = configNode["accessId"];
                setting.AccessSecret = configNode["accessSecret"];
                setting.RegionId = configNode["regionId"];
            }
        }
#endif
    }
}
