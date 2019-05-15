// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Fireasy.Data.Entity
{
    public sealed class EntityRepositoryHolder
    {
        private Dictionary<Type, IRepository> holders = new Dictionary<Type, IRepository>();

        public IRepository GetDbSet(IContextService service, Type entityType)
        {
            if (!holders.TryGetValue(entityType, out IRepository set))
            {
                var provider = service.InitializeContext.Provider.GetService<IContextProvider>();
                if (provider != null)
                {
                    set = provider.CreateRepositoryProvider(entityType, service).CreateRepository(service.InitializeContext.Options);
                    holders[entityType] = set;
                }
            }

            return set;
        }
    }
}
