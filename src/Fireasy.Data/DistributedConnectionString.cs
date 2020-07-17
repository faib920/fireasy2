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
        /// <param name="connectionString">数据库连接字符串。</param>
        /// <exception cref="ArgumentNullException">connectionString 为 null。</exception>
        public DistributedConnectionString(string connectionString)
            : base(connectionString)
        {
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
