// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Web.Mvc.Rendering;

namespace Fireasy.Web.EasyUI
{
    public static class BaseExtensions
    {

        /// <summary>
        /// 打上一个标记，form 的 clear 方法将忽略此域。
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static ExtendHtmlString MarkNoClear(this ExtendHtmlString htmlHelper)
        {
            htmlHelper.Builder.MergeAttribute("noclear", "true");
            return htmlHelper;
        }


        /// <summary>
        /// 打上一个标记，combobox、combotree 的值暂存到属性 _value 中，设值操作迟延到事件 onLoadSuccess 里进行。
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static ExtendHtmlString MarkDelayedSet(this ExtendHtmlString htmlHelper)
        {
            htmlHelper.Builder.MergeAttribute("delay", "true");
            return htmlHelper;
        }
    }
}
