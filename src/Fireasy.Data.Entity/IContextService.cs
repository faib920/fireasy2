// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;
using System;
using System.Data;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 数据上下文的服务组件。
    /// </summary>
    public interface IContextService : IDisposable
    {
        /// <summary>
        /// 返回 <see cref="EntityContextInitializeContext"/> 对象。
        /// </summary>
        EntityContextInitializeContext InitializeContext { get; }

        /// <summary>
        /// 返回 <see cref="IProvider"/> 对象。
        /// </summary>
        IProvider Provider { get; }

        /// <summary>
        /// 返回 <see cref="IDatabase"/> 对象。
        /// </summary>
        IDatabase Database { get; }

        /// <summary>
        /// 返回指定实体类型的 <see cref="IRepository"/> 对象。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        IRepository GetDbSet(Type entityType);

        /// <summary>
        /// 开启一个事务。
        /// </summary>
        /// <param name="level"></param>
        void BeginTransaction(IsolationLevel level);

        /// <summary>
        /// 提交事务。
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// 回滚事务。
        /// </summary>
        void RollbackTransaction();
    }
}
