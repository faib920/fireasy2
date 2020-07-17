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
    /// 数据库外键信息。
    /// </summary>
    public class ForeignKey : ISchemaMetadata
    {
        /// <summary>
        /// 获取约束分录名称。
        /// </summary>
        public string Catalog { get; set; }

        /// <summary>
        /// 获取约束架构名称。
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 获取表名称。
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 获取字段名称。
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 获取约束名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取父表名称。
        /// </summary>
        public string PKTable { get; set; }

        /// <summary>
        /// 获取主键名称。
        /// </summary>
        public string PKColumn { get; set; }
    }
}
