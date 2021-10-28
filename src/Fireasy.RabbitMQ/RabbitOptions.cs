// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Options;
using Fireasy.Common.Subscribes;
using System;

namespace Fireasy.RabbitMQ
{
    /// <summary>
    /// RabbitMQ 参数。
    /// </summary>
    public class RabbitOptions : IConfiguredOptions
    {
        /// <summary>
        /// 获取或设置配置中的实例名称。
        /// </summary>
        public string ConfigName { get; set; }

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
        public int? Port { get; set; }

        /// <summary>
        /// 获取或设置自定义的序列化类。继承自 <see cref="RabbitSerializer"/> 类。
        /// </summary>
        public Type SerializerType { get; set; }

        /// <summary>
        /// 获取或设置交换机类型。默认为空时，消息只会通知给一个消费客户端；当使用 topic 时消息将通知给所有消费客户端。
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

        /// <summary>
        /// 获取或设置初始化方法。
        /// </summary>
        public Action<ISubscribeManager> Initializer { get; set; }

        bool IConfiguredOptions.IsConfigured { get; set; }
    }
}
#endif