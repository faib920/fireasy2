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
    /// 指定关联实体类型使用哪一个 <see cref="RelationshipAttribute"/> 进行对应。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RelationshipUseAttribute : Attribute
    {
        /// <summary>
        /// 实例化 <see cref="RelationshipUseAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="foreignKey">关联的实体类型中的充当主键的属性名称。</param>
        public RelationshipUseAttribute(string foreignKey)
        {
            ForeignKey = foreignKey;
        }

        /// <summary>
        /// 获取或设置当前实体类型中充当外键的名称。
        /// </summary>
        public string ForeignKey { get; set; }
    }
}
