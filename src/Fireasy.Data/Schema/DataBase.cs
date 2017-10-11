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
    /// 数据库信息。
    /// </summary>
    public class DataBase : ISchemaMetadata
    {
        /// <summary>
        /// 获取或设置数据库名称。
        /// </summary>
        public string Name { get; set; }
    }
}
