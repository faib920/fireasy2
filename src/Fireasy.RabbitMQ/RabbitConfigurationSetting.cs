// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using System;

namespace Fireasy.RabbitMQ
{
    [ConfigurationSettingParseType(typeof(RabbitConfigurationSettingParser))]
    public class RabbitConfigurationSetting : IConfigurationSettingItem
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Server { get; set; }

        public Type SerializerType { get; set; }

        public string ExchangeType { get; set; }
    }
}
