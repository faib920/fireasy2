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
    /// 元数据集合信息。
    /// </summary>
    public class MetadataCollection : ISchemaMetadata
    {
        /// <summary>
        /// 获取集合名称。
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// 获取集合指定的限制数。
        /// </summary>
        public int NumberOfRestrictions { get; set; }

        /// <summary>
        /// 获取集合复合标识符的个数。
        /// </summary>
        public int NumberOfIdentifierParts { get; set; }
    }
}
