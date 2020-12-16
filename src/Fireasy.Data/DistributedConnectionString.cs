// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data
{
    /// <summary>
    /// 表示分布式的数据库连接串。
    /// </summary>
    public class DistributedConnectionString : ConnectionString
    {
        /// <summary>
        /// 使用连接字符串初始化 <see cref="DistributedConnectionString"/> 类的新实例。
        /// </summary>
        /// <param name="mode">分布式模式。</param>
        /// <param name="connectionString">数据库连接字符串。</param>
        /// <param name="weight">权重。</param>
        /// <exception cref="ArgumentNullException">connectionString 为 null。</exception>
        public DistributedConnectionString(DistributedMode mode, string connectionString, int weight = 100)
            : base(connectionString)
        {
            Mode = mode;
            Weight = weight;
        }

        /// <summary>
        /// 使用连接字符串初始化 <see cref="DistributedConnectionString"/> 类的新实例。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串。</param>
        /// <exception cref="ArgumentNullException">connectionString 为 null。</exception>
        public DistributedConnectionString(string connectionString)
            : base(connectionString)
        {
        }

        public static bool operator ==(DistributedConnectionString connStr1, DistributedConnectionString connStr2)
        {
            if (Equals(connStr1, null) && Equals(connStr2, null))
            {
                return true;
            }

            if ((Equals(connStr1, null) && !Equals(connStr2, null)) || (!Equals(connStr1, null) && Equals(connStr2, null)))
            {
                return false;
            }

            return (ConnectionString)connStr1 == connStr2 && connStr1.Mode == connStr2.Mode && connStr1.Weight == connStr2.Weight;
        }

        public static bool operator !=(DistributedConnectionString connStr1, DistributedConnectionString connStr2)
        {
            if (Equals(connStr1, null) && Equals(connStr2, null))
            {
                return false;
            }

            if ((Equals(connStr1, null) && !Equals(connStr2, null)) || (!Equals(connStr1, null) && Equals(connStr2, null)))
            {
                return true;
            }

            return (ConnectionString)connStr1 != connStr2 || connStr1.Mode != connStr2.Mode || connStr1.Weight != connStr2.Weight;
        }

        /// <summary>
        /// 获取或设置分布式模式。
        /// </summary>
        public DistributedMode Mode { get; set; }

        /// <summary>
        /// 获取或设置权重。
        /// </summary>
        public int Weight { get; set; }
    }
}
