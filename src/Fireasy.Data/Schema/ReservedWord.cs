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
    /// 数据库保留字信息。
    /// </summary>
    public class ReservedWord : ISchemaMetadata
    {
        /// <summary>
        /// 获取保留字名称。
        /// </summary>
        public string Word { get; set; }
    }
}
