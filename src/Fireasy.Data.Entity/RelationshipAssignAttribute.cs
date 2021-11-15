// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 标记通过主外键分配的关系。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RelationshipAssignAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="RelationshipAssignAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="primaryKey">当前实体中的充当外键的属性名称。</param>
        /// <param name="foreignKey">关联的实体类型中的充当主键的属性名称。</param>
        public RelationshipAssignAttribute(string primaryKey, string foreignKey)
        {
            PrimaryKey = primaryKey;
            ForeignKey = foreignKey;
        }

        /// <summary>
        /// 初始化 <see cref="RelationshipAssignAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="foreignKey">关联的实体类型中的充当主键的属性名称。</param>
        public RelationshipAssignAttribute(string foreignKey)
        {
            ForeignKey = foreignKey;
        }

        /// <summary>
        /// 获取或设置关联实体类型中充当主键的属性名称。
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        /// 获取或设置当前实体类型中充当外键的名称。
        /// </summary>
        public string ForeignKey { get; set; }
    }
}
