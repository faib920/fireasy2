// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Provider;
using System;

namespace Fireasy.MongoDB
{
    public sealed class MongoDBContextProvider : IContextProvider
    {
        IProvider IProviderService.Provider { get; set; }

        IRepositoryProvider IContextProvider.Create(Type entityType, object context)
        {
            return typeof(MongoDBRepositoryProvider<>).MakeGenericType(entityType).New<IRepositoryProvider>(context);
        }

        IRepositoryProvider<TEntity> IContextProvider.Create<TEntity>(object context)
        {
            return new MongoDBRepositoryProvider<TEntity>((InternalContext)context);
        }
    }
}
