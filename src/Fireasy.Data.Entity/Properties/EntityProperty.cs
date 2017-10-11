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
    /// 引用自其他实体对象的属性。无法继承此类。
    /// </summary>
    [Serializable]
    [PropertyLazyLoadder(typeof(EntityPropertyLazyLoadder))]
    public sealed class EntityProperty : RelationProperty, IPropertyLazy
    {
        /// <summary>
        /// 初始化 <see cref="EntityProperty"/> 类的新实例。
        /// </summary>
        public EntityProperty()
            : base (RelationPropertyType.Entity)
        {
        }
    }
}
