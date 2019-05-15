// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity;
using Fireasy.Data.Provider;
using System;

namespace Fireasy.MongoDB
{
    public sealed class MongoDBContextProvider : IContextProvider
    {
        IProvider IProviderService.Provider { get; set; }

        IRepositoryProvider IContextProvider.CreateRepositoryProvider(Type entityType, IContextService service)
        {
            return typeof(MongoDBRepositoryProvider<>).MakeGenericType(entityType).New<IRepositoryProvider>(service);
        }

        IRepositoryProvider<TEntity> IContextProvider.CreateRepositoryProvider<TEntity>(IContextService service)
        {
            return new MongoDBRepositoryProvider<TEntity>(service);
        }

        IContextService IContextProvider.CreateContextService(EntityContextInitializeContext context)
        {
            return new MongoDBContextService(context);
        }
    }
}
