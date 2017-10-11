// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;

namespace Fireasy.Data
{
    /// <summary>
    /// 提供从对象类型中获取字段名称及数据类型的方法。
    /// </summary>
    public interface IPropertyFieldMappingResolver
    {
        /// <summary>
        /// 获取字段名称和数据类型的映射字典。
        /// </summary>
        /// <returns></returns>
        IEnumerable<PropertyFieldMapping> GetDbMapping();
    }

    /// <summary>
    /// 对象和字段的映射。
    /// </summary>
    public class PropertyFieldMapping
    {
        /// <summary>
        /// 初始化 <see cref="PropertyFieldMapping"/> 类的新实例。
        /// </summary>
        /// <param name="propertyName">属性的名称。</param>
        /// <param name="fieldName">字段的名称。</param>
        /// <param name="propertyType">属性的类型。</param>
        /// <param name="dbType">字段的数据类型。</param>
        public PropertyFieldMapping(string propertyName, string fieldName, Type propertyType, DbType dbType)
        {
            PropertyName = propertyName;
            FieldName = fieldName;
            PropertyType = propertyType;
            FieldType = dbType;
        }

        /// <summary>
        /// 获取或设置属性的名称。
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 获取或设置字段的名称。
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 获取或设置属性的类型。
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// 获取或设置字段的数据类型。
        /// </summary>
        public DbType FieldType { get; set; }

        /// <summary>
        /// 获取或设置获取值的函数。
        /// </summary>
        public Func<object, object> ValueFunc { get; set; }
    }
}
