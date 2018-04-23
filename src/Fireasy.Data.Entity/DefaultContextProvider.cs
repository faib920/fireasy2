// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Provider;
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 默认的数据上下文服务实现。
    /// </summary>
    public sealed class DefaultContextProvider : IContextProvider
    {
        IProvider IProviderService.Provider { get; set; }

        IRepositoryProvider IContextProvider.Create(Type entityType, InternalContext context)
        {
            return typeof(DefaultRepositoryProvider<>).MakeGenericType(entityType).New<IRepositoryProvider>(context);
        }

        IRepositoryProvider<TEntity> IContextProvider.Create<TEntity>(InternalContext context)
        {
            return new DefaultRepositoryProvider<TEntity>(context);
        }
    }
}
