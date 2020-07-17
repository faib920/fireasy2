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
    /// 一个标识实体如何与数据表进行映射的特性。无法继承此类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class EntityMappingAttribute : Attribute
    {
        /// <summary>
        /// 使用数据表名称初始化 <see cref="EntityMappingAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="tableName">表的名称。</param>
        /// <param name="instanceName">数据库实例的名称。</param>
        public EntityMappingAttribute(string tableName)
        {
            TableName = tableName;
        }

        /// <summary>
        /// 获取或设置实例映射的数据表名称。
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 获取或设置表的注释。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 获取或设置是否只读。
        /// </summary>
        public bool IsReadonly { get; set; }
    }
}