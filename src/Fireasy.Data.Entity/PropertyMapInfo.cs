// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Data;
using System.Reflection;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 标识实体属性的映射信息。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class PropertyMapInfo
    {
        /// <summary>
        /// 初始化 <see cref="PropertyMapInfo"/> 类的新实例。
        /// </summary>
        public PropertyMapInfo()
        {
            IsNullable = true;
        }

        /// <summary>
        /// 获取或设置列的名称。
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 获取或设置列的注释。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 获取或设置列的数据类型。
        /// </summary>
        public DbType? DataType { get; set; }

        /// <summary>
        /// 获取或设置默认值。
        /// </summary>
        public PropertyValue DefaultValue { get; set; }

        /// <summary>
        /// 获取或设置格式。当指定 DefaultValue 且属性类型为 <see cref="String"/> 时，可使用此属性进行格式化。
        /// </summary>
        public string DefaultValueFormatter { get; set; }

        /// <summary>
        /// 获取或设置列的最大长度。
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// 获取或设置数值型列的精度。
        /// </summary>
        public int? Precision { get; set; }

        /// <summary>
        /// 获取或设置数值型列的小数位数。
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// 获取或设置列的自动生成类型。
        /// </summary>
        public IdentityGenerateType GenerateType { get; set; }

        /// <summary>
        /// 获取或设置是否为主键。
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// 获取或设置是否可为空。
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// 获取或设置是否为假删除标识。
        /// </summary>
        public bool IsDeletedKey { get; set; }

        /// <summary>
        /// 获取或设置是否为并发控制标志。
        /// </summary>
        public bool IsConcurrencyKey { get; set; }

        /// <summary>
        /// 获取属性所对应的 <see cref="PropertyInfo"/> 对象。
        /// </summary>
        public PropertyInfo ReflectionInfo { get; internal set; }
    }
}
