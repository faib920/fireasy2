// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data;
using System;
using System.Collections;
#if !NETCOREAPP
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace Fireasy.Web.EasyUI
{
    /// <summary>
    /// EasyUI 的辅助类。
    /// </summary>
    public static class EasyUIHelper
    {
#if !NETCOREAPP
        /// <summary>
        /// 构造分页数据。
        /// </summary>
        /// <returns></returns>
        public static DataPager GetDataPager()
        {
            var request = HttpContext.Current.Request;
            if (string.IsNullOrEmpty(request.Params["page"]))
            {
                return null;
            }

            var page = request.Params["page"].To(1);
            var pageSize = request.Params["rows"].To(10);
            return new DataPager(pageSize, page - 1);
        }

        /// <summary>
        /// 获取排序信息。
        /// </summary>
        /// <returns></returns>
        public static SortDefinition GetSorting()
        {
            var def = new SortDefinition();
            var request = HttpContext.Current.Request;
            def.Member = request.Params["sort"];
            if (string.IsNullOrEmpty(request.Params["order"]))
            {
                def.Order = SortOrder.None;
            }
            else
            {
                def.Order = request.Params["order"] == "desc" ? SortOrder.Descending : SortOrder.Ascending;
            }

            return def;
        }
#endif

        /// <summary>
        /// 构造分页数据。
        /// </summary>
        /// <returns></returns>
        public static DataPager GetDataPager(this HttpContext context)
        {
            var request = context.Request;
#if NETCOREAPP
            if (!request.HasFormContentType)
            {
                return null;
            }
#endif

            if (string.IsNullOrEmpty(request.Form["page"]))
            {
                return null;
            }

            var page = Convert.ToInt32(request.Form["page"]);
            var pageSize = Convert.ToInt32(request.Form["rows"]);
            return new DataPager(pageSize, page - 1);
        }

        /// <summary>
        /// 获取排序信息。
        /// </summary>
        /// <returns></returns>
        public static SortDefinition GetSorting(this HttpContext context)
        {
            var def = new SortDefinition();
            var request = context.Request;
#if NETCOREAPP
            if (!request.HasFormContentType)
            {
                return def;
            }
#endif

            def.Member = request.Form["sort"];
            if (string.IsNullOrEmpty(request.Form["order"]))
            {
                def.Order = SortOrder.None;
            }
            else
            {
                def.Order = request.Form["order"] == "desc" ? SortOrder.Descending : SortOrder.Ascending;
            }

            return def;
        }

        /// <summary>
        /// 对数据进行分页转换处理。
        /// </summary>
        /// <param name="pager">分页对象。</param>
        /// <param name="data">要输出的数据。</param>
        /// <param name="footer">页脚的统计数据。</param>
        /// <returns></returns>
        public static object Transfer(DataPager pager, IEnumerable data, object footer = null)
        {
            if (pager == null)
            {
                if (footer == null)
                {
                    return data;
                }
                else
                {
                    var f = footer is IEnumerable ? footer : new[] { footer };
                    return new { rows = data, footer = f };
                }
            }
            else
            {
                var f = footer is IEnumerable ? footer : new[] { footer };
                return new { total = pager.RecordCount, rows = data, footer = f };
            }
        }
    }
}
