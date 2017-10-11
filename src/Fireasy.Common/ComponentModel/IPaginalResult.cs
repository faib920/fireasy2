// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;

namespace Fireasy.Common.ComponentModel
{
    public interface IPaginalResult
    {
        /// <summary>
        /// 获取或设置记录数。
        /// </summary>
        int RecordCount { get; set; }

        /// <summary>
        /// 返回或设置总页数。
        /// </summary>
        int PageCount { get; set; }

        /// <summary>
        /// 获取或设置当前页的记录。
        /// </summary>
        IEnumerable Data { get; set; }
    }

    public interface IPaginalResult<T> : IPaginalResult
    {

        /// <summary>
        /// 获取或设置当前页的记录。
        /// </summary>
        IEnumerable<T> Data { get; set; }
    }
}
