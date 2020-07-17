// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Reflection;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 
    /// </summary>
    public class EntityRepositoryTypeMapper
    {
        public EntityRepositoryTypeMapper(PropertyInfo property, Type entityType)
        {
            Property = property;
            EntityType = entityType;
        }

        public PropertyInfo Property { get; }

        public Type EntityType { get; }
    }
}
