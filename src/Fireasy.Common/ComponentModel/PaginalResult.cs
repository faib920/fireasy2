// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;
using System.Collections.Generic;

namespace Fireasy.Common.ComponentModel
{
    /// <summary>
    /// 提供一个带有分页信息的泛型返回结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPaginalResult<T>
    {
        /// <summary>
        /// 获取或设置页码总数。
        /// </summary>
        int Pages { get; set; }

        /// <summary>
        /// 获取或设置记录总数。
        /// </summary>
        int Total { get; set; }

        /// <summary>
        /// 获取或设置是否为最后一页。
        /// </summary>
        bool IsEnd { get; set; }

        /// <summary>
        /// 获取或设置客户端接收的数据列表。
        /// </summary>
        [TextSerializeElement("data")]
        IEnumerable<T> Data { get; set; }
    }

    /// <summary>
    /// 提供一个带有分页信息的泛型返回结构。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginalResult<T> : Result<IEnumerable<T>>, IPaginalResult<T>
    {
        /// <summary>
        /// 初始化 <see cref="PaginalResult{T}"/> 类的新实例。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pager"></param>
        public PaginalResult(IEnumerable<T> data, IPager pager)
        {
            Data = data;
            Succeed = true;

            if (pager != null)
            {
                Pages = pager.PageCount;
                Total = pager.RecordCount;
                IsEnd = pager.CurrentPageIndex >= pager.PageCount - 1;
            }
        }

        /// <summary>
        /// 获取或设置页码总数。
        /// </summary>
        [TextSerializeElement("pages")]
        public int Pages { get; set; }

        /// <summary>
        /// 获取或设置记录总数。
        /// </summary>
        [TextSerializeElement("total")]
        public int Total { get; set; }

        /// <summary>
        /// 获取或设置是否为最后一页。
        /// </summary>
        [TextSerializeElement("isEnd")]
        public bool IsEnd { get; set; }
    }
}
