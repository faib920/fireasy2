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

        public EntityContext EntityContext { get; }

        public IContextService ContextService { get; }

        public ReadOnlyCollection<EntityRepositoryTypeMapper> Mappers { get; }
    }

    public sealed class EntityContextInitializeContext
    {
        internal EntityContextInitializeContext(EntityContext context, IContextService service)
        {
            EntityContext = context;
            ContextService = service;
        }

        public EntityContext EntityContext { get; }

        public IContextService ContextService { get; }
    }
}
