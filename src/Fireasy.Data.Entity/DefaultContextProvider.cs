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
using System.Reflection;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 默认的数据上下文服务实现。
    /// </summary>
    public sealed class DefaultContextProvider : IContextProvider
    {
        IProvider IProviderService.Provider { get; set; }

        IRepositoryProvider IContextProvider.Create(Type entityType, object context)
        {
            var constructor = typeof(DefaultRepositoryProvider<>)
                .MakeGenericType(entityType).GetConstructors()[0];

            return (IRepositoryProvider)constructor.FastInvoke(context);
        }

        IRepositoryProvider<TEntity> IContextProvider.Create<TEntity>(object context)
        {
            return new DefaultRepositoryProvider<TEntity>((InternalContext)context);
        }
    }
}
