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
    /// 数据库表信息。
    /// </summary>
    public class Table : ISchemaMetadata
    {
        /// <summary>
        /// 获取分录名称。
        /// </summary>
        public string Catalog { get; set; }

        /// <summary>
        /// 获取架构名称。
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 获取表名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取表类型。
        /// </summary>
        public TableType Type { get; set; }

        /// <summary>
        /// 获取表的描述。
        /// </summary>
        public string Description { get; set; }
    }

    public enum TableType
    {
        BaseTable,
        SystemTable
    }
}
