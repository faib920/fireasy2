// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity.Properties
{
    /// <summary>
    /// 通过实体关系引用另一个实体的属性。无法继承此类。
    /// </summary>
    [PropertyLazyLoadder(typeof(ReferencePropertyLazyLoadder))]
    public sealed class ReferenceProperty : RelationProperty, IPropertyReference
    {
        /// <summary>
        /// 初始化 <see cref="ReferenceProperty"/> 类的新实例。
        /// </summary>
        public ReferenceProperty()
            : base (RelationPropertyType.RefProperty)
        {
        }

        /// <summary>
        /// 获取或设置参照的属性。
        /// </summary>
        public IProperty Reference { get; set; }
    }
}
