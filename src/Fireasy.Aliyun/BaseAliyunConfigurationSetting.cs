using Fireasy.Common.Configuration;

namespace Fireasy.Aliyun
{
    public class BaseAliyunConfigurationSetting : IConfigurationSettingItem
    {
        public string AccessId { get; set; }

        public string AccessSecret { get; set; }

        public string RegionId { get; set; }
    }
}
