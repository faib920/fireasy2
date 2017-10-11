// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Fireasy.Data.Entity.Properties
{
    /// <summary>
    /// 引用自一个子实体集合的属性。无法继承此类。
    /// </summary>
    [Serializable]
    [PropertyLazyLoadder(typeof(EntitySetPropertyLazyLoadder))]
    public sealed class EntitySetProperty : RelationProperty, IPropertyLazy
    {
        /// <summary>
        /// 初始化 <see cref="EntitySetProperty"/> 类的新实例。
        /// </summary>
        public EntitySetProperty()
            : base (RelationPropertyType.EntitySet)
        {
        }
    }
}
