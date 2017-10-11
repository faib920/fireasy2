// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;

namespace Fireasy.Web.Mvc
{
    public static class RadioGroupExtensions
    {
        public static ExtendHtmlString RadioGroup(this HtmlHelper htmlHelper, string exp, RadioGroupSettings settings = null)
        {
            var builder = new TagBuilder("div");
            builder.AddCssClass("radio-group");
            builder.GenerateId("rgp" + exp);
            builder.InnerHtml = BuildRadioOptions(exp, settings);
            return new ExtendHtmlString(builder);
        }

        public static ExtendHtmlString RadioGroup(this HtmlHelper htmlHelper, string exp, Type enumType, RadioGroupSettings settings = null)
        {
            var builder = new TagBuilder("div");
            builder.AddCssClass("radio-group");
            builder.GenerateId("rgp" + exp);
            builder.InnerHtml = BuildRadioOptions(exp, enumType, settings);
            return new ExtendHtmlString(builder);
        }

        public static ExtendHtmlString RadioGroup<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, RadioGroupSettings settings = null)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var propertyName = metadata.PropertyName;
            var builder = new TagBuilder("div");
            builder.AddCssClass("radio-group");
            builder.GenerateId("rgp" + propertyName);
            builder.InnerHtml = BuildRadioOptions(propertyName, settings);
            return new ExtendHtmlString(builder);
        }

        public static ExtendHtmlString RadioGroup<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, Type enumType, RadioGroupSettings settings = null)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var propertyName = metadata.PropertyName;
            var builder = new TagBuilder("div");
            builder.AddCssClass("radio-group");
            builder.GenerateId("rgp" + propertyName);
            builder.InnerHtml = BuildRadioOptions(propertyName, enumType, settings);
            return new ExtendHtmlString(builder);
        }

        private static string BuildRadioOptions(string name, RadioGroupSettings settings)
        {
            if (settings == null || settings.Items == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var kvp in settings.Items)
            {
                var chk = settings.Value != null && settings.Value.ToString() == kvp.Key
                    ? " checked=\"checked\"" : string.Empty;

                sb.AppendFormat("<div><input type=\"radio\" id=\"{0}_{1}\" name=\"{0}\" value=\"{1}\"{3} /><label for=\"{0}_{1}\">{2}</label></div>",
                    name, kvp.Key, kvp.Value, chk);
            }

            return sb.ToString();
        }

        private static string BuildRadioOptions(string name, Type enumType, RadioGroupSettings settings)
        {
            var sb = new StringBuilder();
            foreach (var kvp in enumType.GetEnumList())
            {
                var chk = settings != null && settings.Value != null && settings.Value.To<int>() == kvp.Key
                    ? " checked=\"checked\"" : string.Empty;

                sb.AppendFormat("<div><input type=\"radio\" id=\"{3}_{0}_{1}\" name=\"{3}\" value=\"{1}\"{4} /><label for=\"{3}_{0}_{1}\">{2}</label></div>",
                    enumType.Name, kvp.Key, kvp.Value, name, chk);
            }

            return sb.ToString();
        }
    }
}
