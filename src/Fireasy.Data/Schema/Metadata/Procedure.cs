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
    /// 数据库存储过程信息。
    /// </summary>
    public class Procedure : ISchemaMetadata
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
        /// 获取存储过程名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取存储过程类型。
        /// </summary>
        public string Type { get; set; }
    }
}
