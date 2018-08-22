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
    /// 关系型属性，该属性不属于数据结构的范围，而是一类由实体关系所衍生的辅助属性。
    /// </summary>
    public abstract class RelationProperty : IProperty
    {
        /// <summary>
        /// 初始化 <see cref="RelationProperty"/> 类的新实例。
        /// </summary>
        protected RelationProperty()
        {
            RelationPropertyType = RelationPropertyType.Other;
            Options = RelationOptions.Default;
        }

        /// <summary>
        /// 初始化 <see cref="RelationProperty"/> 类的新实例。
        /// </summary>
        /// <param name="relationType"></param>
        internal RelationProperty(RelationPropertyType relationType)
        {
            RelationPropertyType = relationType;
        }

        /// <summary>
        /// 获取或设置属性的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置属性的类型。
        /// </summary>
        public virtual Type Type { get; set; }

        /// <summary>
        /// 获取或设置实体类型。
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// 获取或设置属性的映射信息。
        /// </summary>
        public PropertyMapInfo Info { get; set; }

        /// <summary>
        /// 获取或设置关联的实体类型。
        /// </summary>
        public Type RelationType { get; set; }

        /// <summary>
        /// 获取或设置关联键。
        /// </summary>
        public string RelationKey { get; set; }

        /// <summary>
        /// 获取关联属性类型。
        /// </summary>
        internal RelationPropertyType RelationPropertyType { get; private set; }

        /// <summary>
        /// 获取或设置关联选项。
        /// </summary>
        public RelationOptions Options { get; set; }
    }
}
