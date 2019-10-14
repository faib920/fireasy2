// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Web.Mvc;
using System;
using System.Linq.Expressions;
using System.Text;
#if !NETCOREAPP
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#endif

namespace Fireasy.Web.EasyUI
{
    public static class ComboBoxExtensions
    {
        /// <summary>
        /// 为 <see cref="HtmlHelper"/> 对象扩展 combobox 元素。
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="exp">属性名或使用 cbo 作为前缀的 ID 名称。</param>
        /// <param name="settings">参数选项。</param>
        /// <returns></returns>
        public static ExtendHtmlString ComboBox(this
#if !NETCOREAPP
            HtmlHelper htmlHelper
#else
            IHtmlHelper htmlHelper
#endif
            , string exp, ComboBoxSettings settings = null, object htmlAttributes = null)
        {
            var builder = new EasyUITagBuilder("select", "easyui-combobox", settings);
            builder.MergeAttribute("name", exp);
            builder.MergeAttribute("data-options", SettingsSerializer.Serialize(settings));
            builder.GenerateId("cbo" + exp);
            return new ExtendHtmlString(builder);
        }

        /// <summary>
        /// 为 <see cref="HtmlHelper"/> 对象扩展 combobox 元素。
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="exp">属性名或使用 cbo 作为前缀的 ID 名称。</param>
        /// <param name="enumType">枚举类型。</param>
        /// <param name="settings">参数选项。</param>
        /// <returns></returns>
        public static ExtendHtmlString ComboBox(this
#if !NETCOREAPP
            HtmlHelper htmlHelper
#else
            IHtmlHelper htmlHelper
#endif
            , string exp, Type enumType, ComboBoxSettings settings = null)
        {
            settings = settings ?? new ComboBoxSettings();

            var builder = new EasyUITagBuilder("select", "easyui-combobox", settings);
            builder.MergeAttribute("name", exp);
            builder.MergeAttribute("data-options", SettingsSerializer.Serialize(settings));
            builder.AddCssClass("form-input");
            builder.SetInnerHtmlContent(BuildEnumOptions(enumType));
            builder.GenerateId("cbo" + exp);
            return new ExtendHtmlString(builder);
        }

        /// <summary>
        /// 为 <see cref="HtmlHelper"/> 对象扩展 combobox 元素。
        /// </summary>
        /// <typeparam name="TModel">数据模型类型。</typeparam>
        /// <typeparam name="TProperty">绑定的属性的类型。</typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression">指定绑定属性的表达式。</param>
        /// <param name="settings">参数选项。</param>
        /// <returns></returns>
        public static ExtendHtmlString ComboBox<TModel, TProperty>(this
#if !NETCOREAPP
            HtmlHelper<TModel> htmlHelper
#else
            IHtmlHelper<TModel> htmlHelper
#endif
            , Expression<Func<TModel, TProperty>> expression, ComboBoxSettings settings = null)
        {
            settings = settings ?? new ComboBoxSettings();

            var propertyName = MetadataHelper.GetPropertyName(expression);
            settings.Bind(typeof(TModel), propertyName);

            var builder = new EasyUITagBuilder("select", "easyui-combobox", settings);
            builder.MergeAttribute("name", propertyName);
            builder.MergeAttribute("data-options", SettingsSerializer.Serialize(settings));
            builder.AddCssClass("form-input");
            builder.GenerateId("cbo" + propertyName);
            return new ExtendHtmlString(builder);
        }

        /// <summary>
        /// 为 <see cref="HtmlHelper"/> 对象扩展 validatebox 元素。使用一个枚举类型输出 Key-Value 数组，Value 是使用 <see cref="EnumDescriptionAttribute"/> 标注的。
        /// </summary>
        /// <typeparam name="TModel">数据模型类型。</typeparam>
        /// <typeparam name="TProperty">绑定的属性的类型。</typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression">指定绑定属性的表达式。</param>
        /// <param name="enumType">要绑定的枚举类型。</param>
        /// <param name="settings">参数选项。</param>
        /// <returns></returns>
        public static ExtendHtmlString ComboBox<TModel, TProperty>(this
#if !NETCOREAPP
            HtmlHelper<TModel> htmlHelper
#else
            IHtmlHelper<TModel> htmlHelper
#endif
            , Expression<Func<TModel, TProperty>> expression, Type enumType, ComboBoxSettings settings = null)
        {
            settings = settings ?? new ComboBoxSettings();

            var propertyName = MetadataHelper.GetPropertyName(expression);
            settings.Bind(typeof(TModel), propertyName);

            var builder = new EasyUITagBuilder("select", "easyui-combobox", settings);
            builder.MergeAttribute("name", propertyName);
            builder.MergeAttribute("data-options", SettingsSerializer.Serialize(settings));
            builder.AddCssClass("form-input");
            builder.SetInnerHtmlContent(BuildEnumOptions(enumType));
            builder.GenerateId("cbo" + propertyName);
            return new ExtendHtmlString(builder);
        }

        private static string BuildEnumOptions(Type enumType)
        {
            var sb = new StringBuilder();
            foreach (var kvp in enumType.GetEnumList())
            {
                sb.AppendFormat("<option value=\"{0}\">{1}</option>", kvp.Key, kvp.Value);
            }

            return sb.ToString();
        }
    }
}
