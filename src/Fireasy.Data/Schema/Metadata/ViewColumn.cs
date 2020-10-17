// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// 视图的列信息。
    /// </summary>
    public class ViewColumn : ISchemaMetadata
    {
        /// <summary>
        /// 获取表分录名称。
        /// </summary>
        public string Catalog { get; set; }

        /// <summary>
        /// 获取表架构名称。
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 获取表名称。
        /// </summary>
        public string ViewName { get; set; }

        /// <summary>
        /// 获取列名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取列描述。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 获取数据类型。
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 获取默认值。
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// 获取小数精度。
        /// </summary>
        public int? NumericPrecision { get; set; }

        /// <summary>
        /// 获取小数范围。
        /// </summary>
        public int? NumericScale { get; set; }

        /// <summary>
        /// 获取列长度。
        /// </summary>
        public long? Length { get; set; }

        /// <summary>
        /// 获取是否可为空。
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// 获取是否为主键值。
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// 获取是否为自增长型列。
        /// </summary>
        public bool Autoincrement { get; set; }

        /// <summary>
        /// 获取列的位置。
        /// </summary>
        public int Position { get; set; }
    }
}
