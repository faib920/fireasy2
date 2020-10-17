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
    /// 索引的信息。
    /// </summary>
    public class Index : ISchemaMetadata
    {
        /// <summary>
        /// 获取索引分录名称。
        /// </summary>
        public string Catalog { get; set; }

        /// <summary>
        /// 获取索引架构名称。
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 获取表名称。
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 获取索引名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取是否唯一。
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// 获取是否主键。
        /// </summary>
        public bool IsPrimaryKey { get; set; }
    }
}
