// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实例标识。
    /// </summary>
    public interface IInstanceIdentification
    {
        /// <summary>
        /// 获取或设置数据库提供者实例。
        /// </summary>
        IProvider Provider { get; set; }

        /// <summary>
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 获取或设置数据库连接串。
        /// </summary>
        ConnectionString ConnectionString { get; set; }
    }
}
