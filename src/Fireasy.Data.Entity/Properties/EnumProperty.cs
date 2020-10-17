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
    /// 引用一个枚举类型的文本说明的属性。无法继承此类。
    /// </summary>
    [Serializable]
    [PropertyLazyLoadder(typeof(EnumPropertyLazyLoadder))]
    public sealed class EnumProperty : RelationProperty, IPropertyReference
    {
        /// <summary>
        /// 初始化 <see cref="EnumProperty"/> 类的新实例。
        /// </summary>
        public EnumProperty()
            : base(RelationPropertyType.Enum)
        {
        }

        /// <summary>
        /// 获取或设置参照属性，该属性应该是枚举类型的属性。
        /// </summary>
        public IProperty Reference { get; set; }

    }
}
