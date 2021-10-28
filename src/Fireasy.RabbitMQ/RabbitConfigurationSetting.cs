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
        /// 获取或设置服务器端口号。
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 获取或设置对象序列化器的类型。
        /// </summary>
        public Type SerializerType { get; set; }

        /// <summary>
        /// 获取或设置交换机类型。默认为空时，使用用工作队列；否则为广播队列，取值为 direct、fanout 或 topic。
        /// </summary>
        public string ExchangeType { get; set; }

        /// <summary>
        /// 获取或设置交换机名。
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// 获取或设置虚拟主机。
        /// </summary>
        public string VirtualHost { get; set; }

        /// <summary>
        /// 获取或设置异常时重入队列的延迟时间。默认为 20 秒，未指定表示不重入队列。
        /// </summary>
        public TimeSpan RetryDelayTime { get; set; }

        /// <summary>
        /// 获取或设置可重试的次数。未指定表示不受限制。
        /// </summary>
        public int? RetryTimes { get; set; }
    }
}
