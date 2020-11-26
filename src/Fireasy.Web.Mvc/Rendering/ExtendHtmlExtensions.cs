// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Web.Mvc.Rendering
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

        /// <summary>
        /// 使当前的元素成为只读。
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static ExtendHtmlString Readonly(this ExtendHtmlString htmlHelper)
        {
            return htmlHelper.AddAttribute("readonly", "readonly");
        }

        /// <summary>
        /// 使当前的元素成为不可用。
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static ExtendHtmlString Disable(this ExtendHtmlString htmlHelper)
        {
            return htmlHelper.AddAttribute("disabled", "disabled");
        }

        /// <summary>
        /// 添加属性。
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="attributeName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ExtendHtmlString AddAttribute(this ExtendHtmlString htmlHelper, string attributeName, string value)
        {
            htmlHelper.Builder.MergeAttribute(attributeName, value);
            return htmlHelper;
        }

        /// <summary>
        /// 移除属性。
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static ExtendHtmlString RemoveAttribute(this ExtendHtmlString htmlHelper, string attributeName)
        {
            htmlHelper.Builder.Attributes.Remove(attributeName);
            return htmlHelper;
        }
    }
}
