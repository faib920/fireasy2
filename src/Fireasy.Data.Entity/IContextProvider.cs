// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Provider;
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 数据上下文的服务接口。
    /// </summary>
    public interface IContextProvider : IProviderService
    {
        /// <summary>
        /// 创建仓储提供者对象。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="context"></param>
        /// <returns></returns>
        IRepositoryProvider Create(Type entityType, object context);

        /// <summary>
        /// 创建仓储提供者对象。
        /// </summary>
        /// <typeparam name="TEntity">实体类型。</typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        IRepositoryProvider<TEntity> Create<TEntity>(object context) where TEntity : IEntity;
    }
}
