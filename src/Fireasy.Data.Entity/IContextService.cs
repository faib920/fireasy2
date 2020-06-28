// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Data.Provider;
using System;
using System.Data;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 数据上下文的服务组件。
    /// </summary>
    public interface IContextService : IDisposable, IServiceProviderAccessor, IProviderAware, IContextTypeAware
    {
        /// <summary>
        /// 获取 <see cref="EntityContextOptions"/> 实例。
        /// </summary>
        EntityContextOptions Options { get; }

        /// <summary>
        /// 创建实体类型所对应的 <see cref="IRepositoryProvider"/> 实例。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        IRepositoryProvider CreateRepositoryProvider(Type entityType);

        /// <summary>
        /// 创建实体类型所对应的 <see cref="IRepositoryProvider{TEntity}"/> 实例。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IRepositoryProvider<TEntity> CreateRepositoryProvider<TEntity>() where TEntity : class, IEntity;

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
