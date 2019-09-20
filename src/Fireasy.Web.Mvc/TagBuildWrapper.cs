// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETCOREAPP
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
#endif

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 对 <see cref="TagBuilder"/> 的包装。
    /// </summary>
    public class TagBuildWrapper : TagBuilder
    {
        /// <summary>
        /// 初始化 <see cref="TagBuildWrapper"/> 类的新实例。
        /// </summary>
        /// <param name="tagName"></param>
        public TagBuildWrapper(string tagName)
            : base (tagName)
        {
        }

        /// <summary>
        /// 设置内部的 Html 内容。
        /// </summary>
        /// <param name="html"></param>
        public void SetInnerHtmlContent(string html)
        {
#if !NETCOREAPP
            InnerHtml = html;
#else
            InnerHtml.AppendHtml(html);
#endif
        }

#if NETCOREAPP
        /// <summary>
        /// 生成ID。
        /// </summary>
        /// <param name="name"></param>
        public void GenerateId(string name)
        {
            base.GenerateId(name, string.Empty);
        }
#endif
    }
}
