// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity
{
    internal class EntityExecuteWrapper
    {
        public EntityExecuteWrapper(IEntity entity)
        {
            Entity = entity;
        }

        public IEntity Entity { get; }

        public bool IsEfficient { get; set; }
    }
}
