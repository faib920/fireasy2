// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Provider;
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 默认的数据上下文服务提供者。
    /// </summary>
    public sealed class DefaultContextProvider : IContextProvider
    {
        IProvider IProviderService.Provider { get; set; }

        IRepositoryProvider IContextProvider.CreateRepositoryProvider(Type entityType, IContextService service)
        {
            var constructor = typeof(DefaultRepositoryProvider<>)
                .MakeGenericType(entityType).GetConstructors()[0];

            return (IRepositoryProvider)constructor.FastInvoke(service);
        }

        IRepositoryProvider<TEntity> IContextProvider.CreateRepositoryProvider<TEntity>(IContextService service)
        {
            return new DefaultRepositoryProvider<TEntity>(service);
        }

        IContextService IContextProvider.CreateContextService(EntityContextInitializeContext context)
        {
            return new DefaultContextService(context, context.DatabaseFactory);
        }
    }
}
