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
        /// <summary>
        /// 获取或设置登录账号。
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 获取或设置登录密码。
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 获取或设置服务器地址。
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 获取或设置自定义的序列化类。继承自 <see cref="RabbitSerializer"/> 类。
        /// </summary>
        public Type SerializerType { get; set; }

        /// <summary>
        /// 获取或设置交换机类型。默认为空时，消息只会通知给一个消费客户端；当使用 topic 时消息将通知给所有消费客户端。
        /// </summary>
        public string ExchangeType { get; set; }

        /// <summary>
        /// 获取或设置异常时重入队列的延迟时间（毫秒）。默认为 1000 毫秒。
        /// </summary>
        public int? RequeueDelayTime { get; set; }
    }
}
