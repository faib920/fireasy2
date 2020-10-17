// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 提供解释文本输出的接口。
    /// </summary>
    public interface IQueryExportation
    {
        /// <summary>
        /// 获取查询解释文本。
        /// </summary>
        string QueryText { get; }
    }
}
