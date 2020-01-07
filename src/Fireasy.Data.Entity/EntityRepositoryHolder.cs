// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System;

namespace Fireasy.Data.Entity
{
    public sealed class EntityRepositoryHolder
    {
        private SafetyDictionary<Type, IRepository> holders = new SafetyDictionary<Type, IRepository>();

        public IRepository GetDbSet(IContextService service, Type entityType)
        {
            return holders.GetOrAdd(entityType, () =>
            {
                var provider = service.InitializeContext.Provider.GetService<IContextProvider>();
                if (provider != null)
                {
                    return provider.CreateRepositoryProvider(entityType, service).CreateRepository(service.InitializeContext.Options);
                }

                return null;
            });
        }
    }
}
