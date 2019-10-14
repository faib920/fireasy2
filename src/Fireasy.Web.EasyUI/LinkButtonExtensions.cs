// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Web.Mvc;
#if !NETCOREAPP
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#endif

namespace Fireasy.Web.EasyUI
{
    public static class LinkButtonExtensions
    {
        /// <summary>
        /// 为 <see cref="HtmlHelper"/> 对象扩展 linkbutton 元素。
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="id">ID 属性值。</param>
        /// <param name="onClick">单击时执行的 js 脚本。</param>
        /// <param name="settings">参数选项。</param>
        /// <returns></returns>
        public static ExtendHtmlString LinkButton(this
#if !NETCOREAPP
            HtmlHelper htmlHelper
#else
            IHtmlHelper htmlHelper
#endif
            , string id, string onClick, LinkButtonSettings settings = null)
        {
            var builder = new TagBuildWrapper("span");
            builder.AddCssClass("easyui-linkbutton");
            builder.MergeAttribute("data-options", SettingsSerializer.Serialize(settings));
            builder.GenerateId(id);
            return new ExtendHtmlString(builder);
        }
    }
}
