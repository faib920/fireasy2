// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
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
        public RedisConfigurationSetting()
        {
            Hosts = new List<RedisHost>();
        }

        /// <summary>
        /// 获取或设置连接串。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 获取 Redis 主机群。
        /// </summary>
        public List<RedisHost> Hosts { get; private set; }

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
        /// 获取或设置提前延期的时间比例。
        /// </summary>
        public double? AdvanceDelay { get; set; }

        /// <summary>
        /// 获取或设置上锁时间（秒）。默认为 10 秒钟。
        /// </summary>
        public int LockTimeout { get; set; }
    }

    /// <summary>
    /// Redis 主机配置。
    /// </summary>
    public class RedisHost
    {
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
