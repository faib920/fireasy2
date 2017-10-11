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
    /// 表示带有子查询的实体属性。
    /// </summary>
    public class SubqueryProperty : IProperty, ILoadedProperty
    {
        /// <summary>
        /// 获取或设置属性的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置属性的类型。
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 获取或设置实体类型。
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// 获取或设置属性的映射信息。
        /// </summary>
        public PropertyMapInfo Info { get; set; }

        /// <summary>
        /// 获取子查询语句，使用符号 $ 来表示实体类型的别名。
        /// </summary>
        public string Subquery { get; set; }
    }
}
