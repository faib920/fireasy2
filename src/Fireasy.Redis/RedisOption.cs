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

namespace Fireasy.Redis
{
    public class RedisOptionsBase : IConfiguredOptions
    {
        /// <summary>
        /// 获取或设置配置中的实例名称。
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// 获取或设置连接串。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 获取或设置 Redis 主机群，可以指定多个主机。<para>示例：192.168.1.1:6379;192.168.1.2</para>
        /// </summary>
        public string Hosts { get; set; }

        /// <summary>
        /// 获取或设置连接池大小。
        /// </summary>
        public int? PoolSize { get; set; }

        /// <summary>
        /// 获取或设置是否使用SSL连接。
        /// </summary>
        public bool Ssl { get; set; }

        /// <summary>
        /// 获取或设置缺省的数据库编号。
        /// </summary>
        public int DefaultDb { get; set; }

        /// <summary>
        /// 获取或设置写入缓冲区大小。
        /// </summary>
        public int? WriteBuffer { get; set; }

        /// <summary>
        /// 获取或设置密码。
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 获取或设置对象序列化器的类型。
        /// </summary>
        public Type SerializerType { get; set; }

        /// <summary>
        /// 获取或设置是否启用 Twemproxy 代理。
        /// </summary>
        public bool Twemproxy { get; set; }

        /// <summary>
        /// 获取或设置上锁时间（秒）。默认为 10 秒钟。
        /// </summary>
        public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 获取或设置连接超时时间（毫秒）。默认为 5000 毫秒。
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromMilliseconds(5000);

        /// <summary>
        /// 获取或设置发送/接收超时时间（毫秒）。默认为 10000 毫秒。
        /// </summary>
        public TimeSpan SyncTimeout { get; set; } = TimeSpan.FromMilliseconds(10000);

        bool IConfiguredOptions.IsConfigured { get; set; }
    }

    /// <summary>
    /// Redis 缓存参数。
    /// </summary>
    public class RedisCachingOptions : RedisOptionsBase
    {
        /// <summary>
        /// 获取或设置数据库范围，格式如 1,2,3、1-5 或 1,2,4-7,9,10。
        /// </summary>
        public string DbRange { get; set; }

        /// <summary>
        /// 获取或设置对 Key 截取的规则，如 left(4)、right(5)、substr(1, 3)。
        /// </summary>
        public string KeyRule { get; set; }

        /// <summary>
        /// 获取或设置滑行的时间。
        /// </summary>
        public TimeSpan SlidingTime { get; set; }
    }

    /// <summary>
    /// Redis 消息发布与订阅参数。
    /// </summary>
    public class RedisSubscribeOptions : RedisOptionsBase
    {
        /// <summary>
        /// 获取或设置异常时重入队列的延迟时间（毫秒）。未指定表示不重入队列。
        /// </summary>
        public TimeSpan? RequeueDelayTime { get; set; }

        /// <summary>
        /// 获取或设置初始化方法。
        /// </summary>
        public Action<ISubscribeManager> Initializer { get; set; }
    }

    /// <summary>
    /// Redis 分布式事务锁参数。
    /// </summary>
    public class RedisDistributedLockerOptions : RedisOptionsBase
    {

    }
}
#endif