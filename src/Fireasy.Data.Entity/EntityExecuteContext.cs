// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Data.Provider;

namespace Fireasy.Data.Entity
{
    internal class EntityExecuteContext
    {
        public EntityExecuteContext(IProvider provider, IEntity entity, IPropertyFilter filter)
            : this (provider, entity)
        {
            Filter = filter;
        }

        public EntityExecuteContext(IEntity entity, IPropertyFilter filter)
            : this(null, entity, filter)
        {
        }

        public EntityExecuteContext(IEntity entity)
            : this (null, entity)
        {
        }

        public EntityExecuteContext(IProvider provider, IEntity entity)
        {
            Provider = provider;
            Entity = entity;
        }

        public IProvider Provider { get; }

        public IEntity Entity { get; }

        public IPropertyFilter Filter { get; }

        public bool IsEfficient { get; set; }
    }
}
