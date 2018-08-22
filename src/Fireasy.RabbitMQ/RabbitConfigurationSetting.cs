using Fireasy.Common.Configuration;
using System;

namespace Fireasy.RabbitMQ
{
    public class RabbitConfigurationSetting : IConfigurationSettingItem
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Server { get; set; }

        public Type SerializerType { get; set; }
    }
}
