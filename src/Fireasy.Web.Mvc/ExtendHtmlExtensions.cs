// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Web.Mvc
{
    public static class ExtendHtmlExtensions
    {
        /// <summary>
        /// 添加样式。
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static ExtendHtmlString Style(this ExtendHtmlString htmlHelper, string style)
        {
            htmlHelper.Builder.MergeAttribute("style", style);
            return htmlHelper;
        }

        /// <summary>
        /// 添加 css 类名。
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static ExtendHtmlString AddCssClass(this ExtendHtmlString htmlHelper, string className)
        {
            htmlHelper.Builder.AddCssClass(className);
            return htmlHelper;
        }
    }
}
