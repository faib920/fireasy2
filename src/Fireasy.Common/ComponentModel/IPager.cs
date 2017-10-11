// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.ComponentModel
{
    /// <summary>
    /// 表示提供分页。
    /// </summary>
    public interface IPager
    {
        /// <summary>
        /// 获取或设置每页的记录数。
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// 获取或设置当前页码，该值从0开始。
        /// </summary>
        int CurrentPageIndex { get; set; }

        /// <summary>
        /// 返回或设置总页数。
        /// </summary>
        int PageCount { get; set; }

        /// <summary>
        /// 获取或设置记录数。
        /// </summary>
        int RecordCount { get; set; }
    }
}
