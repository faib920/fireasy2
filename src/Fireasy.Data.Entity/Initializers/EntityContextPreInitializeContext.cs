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
    public sealed class EntityContextPreInitializeContext
    {
        internal EntityContextPreInitializeContext(EntityContext context, IContextService service, List<EntityRepositoryTypeMapper> mappers)
        {
            EntityContext = context;
            ContextService = service;
            Mappers = new ReadOnlyCollection<EntityRepositoryTypeMapper>(mappers);
        }

        public EntityContext EntityContext { get; private set; }

        public IContextService ContextService { get; private set; }

        public ReadOnlyCollection<EntityRepositoryTypeMapper> Mappers { get; private set; }
    }
}
