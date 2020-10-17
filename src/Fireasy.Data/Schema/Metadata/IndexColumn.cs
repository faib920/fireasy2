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
    /// 索引列的信息。
    /// </summary>
    public class IndexColumn : ISchemaMetadata
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
        /// 获取表的名称。
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 获取索引的名称。
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// 获取列的名称。
        /// </summary>
        public string ColumnName { get; set; }
    }
}
