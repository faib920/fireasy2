﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Data.Common;

namespace Fireasy.Data
{
    /// <summary>
    /// 提供分布式数据库连接支持。
    /// </summary>
    public interface IDistributedDatabase : IDatabase
    {
        /// <summary>
        /// 获取分布式数据库连接字符串组。
        /// </summary>
        ReadOnlyCollection<DistributedConnectionString> DistributedConnectionStrings { get; }

        /// <summary>
        /// 获取指定模式的 <see cref="DbConnection"/> 对象。
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        DbConnection GetConnection(DistributedMode mode);
    }
}
