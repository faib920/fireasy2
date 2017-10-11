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
    /// 关系型属性类型。
    /// </summary>
    internal enum RelationPropertyType
    {
        /// <summary>
        /// 引用实体属性类型。
        /// </summary>
        Entity,

        /// <summary>
        /// 实体集属性类型。
        /// </summary>
        EntitySet,

        /// <summary>
        /// 枚举转换属性类型。
        /// </summary>
        Enum,

        /// <summary>
        /// 引用属性类型。
        /// </summary>
        RefProperty,

        /// <summary>
        /// 其他。
        /// </summary>
        Other
    }
}
