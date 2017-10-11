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
    /// 集合的限制信息。
    /// </summary>
    public class Restriction : ISchemaMetadata
    {
        /// <summary>
        /// 获取限制的集合名称。
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// 获取限制的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取在限制集合中的实际位置。
        /// </summary>
        public int Number { get; set; }
    }
}
