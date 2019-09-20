// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Fireasy.Data.Entity.Initializers
{
    public class EntityContextPreInitializeContext
    {
        internal EntityContextPreInitializeContext(EntityContext context, IContextService service, List<EntityRepositoryMapper> mappers)
        {
            EntityContext = context;
            Service = service;
            Mappers = new ReadOnlyCollection<EntityRepositoryMapper>(mappers);
        }

        public EntityContext EntityContext { get; private set; }

        public IContextService Service { get; private set; }

        public ReadOnlyCollection<EntityRepositoryMapper> Mappers { get; private set; }
    }
}
