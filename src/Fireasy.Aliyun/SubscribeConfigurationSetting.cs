using Fireasy.Common.Configuration;
using System;

namespace Fireasy.Aliyun
{
    [ConfigurationSettingParseType(typeof(SubscribeConfigurationSettingParser))]
    public class SubscribeConfigurationSetting : BaseAliyunConfigurationSetting
    {
        /// <summary>
        /// 获取序列化和反序列化的类型。
        /// </summary>
        public Type SerializerType { get; set; }

        /// <summary>
        /// 获取或设置实例ID。
        /// </summary>
        public string InstanceId { get; set; }

    }
}
