// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Data;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 为实体持久化工作区提供一些选项。无法继承此类。
    /// </summary>
    public sealed class EntityTransactionScopeOption
    {
        /// <summary>
        /// 获取或设置事务的锁定行为。
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; }

        /// <summary>
        /// 获取或设置事务超时时间。
        /// </summary>
        public TimeSpan Timeout { get; set; }
    }
}
