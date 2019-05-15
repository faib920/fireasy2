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
    /// 数据上下文的服务接口。
    /// </summary>
    [DefaultProviderService(typeof(DefaultContextProvider))]
    public interface IContextProvider : IProviderService
    {
        /// <summary>
        /// 创建仓储提供者对象。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="service"></param>
        /// <returns></returns>
        IRepositoryProvider CreateRepositoryProvider(Type entityType, IContextService service);

        /// <summary>
        /// 创建仓储提供者对象。
        /// </summary>
        /// <typeparam name="TEntity">实体类型。</typeparam>
        /// <param name="service"></param>
        /// <returns></returns>
        IRepositoryProvider<TEntity> CreateRepositoryProvider<TEntity>(IContextService service) where TEntity : class, IEntity;

        /// <summary>
        /// 创建 <see cref="IContextService"/> 服务组件对象。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IContextService CreateContextService(EntityContextInitializeContext context);
    }
}
