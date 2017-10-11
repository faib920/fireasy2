// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Fireasy.Web.EasyUI
{
    public static class FileBoxExtensions
    {
        /// <summary>
        /// 为 <see cref="HtmlHelper"/> 对象扩展 filebox 元素。
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="exp">属性名或使用 txt 作为前缀的 ID 名称。</param>
        /// <param name="settings">参数选项。</param>
        /// <returns></returns>
        public static HtmlString FileBox(this System.Web.Mvc.HtmlHelper htmlHelper, string exp, FileBoxSettings settings = null)
        {
            var builder = new TagBuilder("input");
            builder.AddCssClass("easyui-filebox");
            builder.MergeAttribute("name", exp);
            builder.MergeAttribute("data-options", SettingsSerializer.Serialize(settings));
            builder.GenerateId("cbo" + exp);
            return new HtmlString(builder.ToString(TagRenderMode.Normal));
        }

        /// <summary>
        /// 为 <see cref="HtmlHelper"/> 对象扩展 filebox 元素。
        /// </summary>
        /// <typeparam name="TModel">数据模型类型。</typeparam>
        /// <typeparam name="TProperty">绑定的属性的类型。</typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression">指定绑定属性的表达式。</param>
        /// <param name="settings">参数选项。</param>
        /// <returns></returns>
        public static HtmlString FileBox<TModel, TProperty>(this System.Web.Mvc.HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, FileBoxSettings settings = null)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var propertyName = metadata.PropertyName;
            var builder = new TagBuilder("input");
            builder.AddCssClass("easyui-filebox");
            builder.MergeAttribute("name", propertyName);
            builder.MergeAttribute("data-options", SettingsSerializer.Serialize(settings));
            builder.GenerateId("cbo" + propertyName);
            return new HtmlString(builder.ToString(TagRenderMode.Normal));
        }
    }
}
