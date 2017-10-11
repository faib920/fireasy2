// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// 数据库数据类型信息。
    /// </summary>
    public class DataType : ISchemaMetadata
    {
        /// <summary>
        /// 获取数据类型名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取数据类型对应的 <see cref="T:System.Data.DbType"/>。
        /// </summary>
        public string DbType { get; set; }

        /// <summary>
        /// 获取数据类型对应的 <see cref="T:System.Type"/>。
        /// </summary>
        public Type SystemType { get; set; }

        /// <summary>
        /// 获取数据类型对应的列长度。
        /// </summary>
        public int ColumnSize { get; set; }

        /// <summary>
        /// 获取此类型的创建格式。
        /// </summary>
        public string CreateFormat { get; set; }

        /// <summary>
        /// 获取创建类型时必须指定的创建参数。
        /// </summary>
        public string CreateParameters { get; set; }

        /// <summary>
        /// 获取是否可自增长。
        /// </summary>
        public bool IsAutoincrementable { get; set; }

        /// <summary>
        /// 获取是否与 .NET Framework 数据类型最佳匹配。
        /// </summary>
        public bool IsBestMatch { get; set; }

        /// <summary>
        /// 获取是否区分大小写。
        /// </summary>
        public bool IsCaseSensitive { get; set; }

        /// <summary>
        /// 获取是否为固定长度。
        /// </summary>
        public bool IsFixedLength { get; set; }

        /// <summary>
        /// 获取是否具有固定的精度和小数位数。
        /// </summary>
        public bool IsFixedPrecisionScale { get; set; }

        /// <summary>
        /// 获取是否包含很长的数据。
        /// </summary>
        public bool IsLong { get; set; }

        /// <summary>
        /// 获取是否可为空。
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// 获取是否可以在包含除 LIKE 谓词以外的任何运算符的 WHERE 子句中使用。
        /// </summary>
        public bool IsSearchable { get; set; }

        /// <summary>
        /// 获取是否可以与 LIKE 谓词一起使用。
        /// </summary>
        public bool IsSearchableWithLike { get; set; }

        /// <summary>
        /// 获取是否为无符号。
        /// </summary>
        public bool IsUnsigned { get; set; }

        /// <summary>
        /// 获取小数点右侧允许的最大位数。
        /// </summary>
        public int MaximumScale { get; set; }

        /// <summary>
        /// 获取小数点右侧允许的最小位数。
        /// </summary>
        public int MinimumScale { get; set; }

        /// <summary>
        /// 获取行更改并且列值与所有以前的值不同时，是否更新数据类型。
        /// </summary>
        public bool IsConcurrencyType { get; set; }

        /// <summary>
        /// 获取是否可以以文本形式表示。
        /// </summary>
        public bool IsLiteralSupported { get; set; }
    }
}
