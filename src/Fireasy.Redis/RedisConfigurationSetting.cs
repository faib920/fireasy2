// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;

namespace Fireasy.Redis
{
    /// <summary>
    /// Redis 的基本配置。
    /// </summary>
    [ConfigurationSettingParseType(typeof(RedisConfigurationSettingParser))]
    public class RedisConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置连接串。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 获取 Redis 主机群。
        /// </summary>
        public List<RedisHost> Hosts { get; private set; } = new List<RedisHost>();

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
        /// 获取或设置数据库范围，格式如 1,2,3、1-5 或 1,2,4-7,9,10。
        /// </summary>
        public string DbRange { get; set; }

        /// <summary>
        /// 获取或设置对 Key 截取的规则，如 left(4)、right(5)、substr(1, 3)。
        /// </summary>
        public string KeyRule { get; set; }

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
        /// 获取或设置提前延期的时间比例。
        /// </summary>
        public double? AdvanceDelay { get; set; }

        /// <summary>
        /// 获取或设置上锁时间（秒）。默认为 10 秒钟。
        /// </summary>
        public int LockTimeout { get; set; }

        /// <summary>
        /// 获取或设置连接超时时间（毫秒）。默认为 5000 毫秒。
        /// </summary>
        public int ConnectTimeout { get; set; } = 5000;

        /// <summary>
        /// 获取或设置发送/接收超时时间（毫秒）。默认为 10000 毫秒。
        /// </summary>
        public int SyncTimeout { get; set; } = 10000;

        /// <summary>
        /// 获取或设置异常时重入队列的延迟时间（毫秒）。未指定表示不重入队列。
        /// </summary>
        public int? RequeueDelayTime { get; set; }
    }

    /// <summary>
    /// Redis 主机配置。
    /// </summary>
    public class RedisHost
    {
        public RedisHost(string server, int port)
        {
            Server = server;
            Port = port;
        }
        public RedisHost(string host)
        {
            int index;
            if ((index = host.IndexOf(':')) != -1)
            {
                Server = host.Substring(0, index);
                Port = host.Substring(index + 1).To<int>();
            }
            else
            {
                Server = host;
            }
        }

        /// <summary>
        /// 获取或设置主机IP。
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// 获取或设置主机端口。
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 获取是否只读。
        /// </summary>
        public bool ReadOnly { get; set; }
    }
}
