using System;
using System.Linq.Expressions;
#if !NETCOREAPP
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#endif

namespace Fireasy.Web.Mvc.Rendering
{
    public static class MetadataHelper
    {
        public static string GetPropertyName<TModel, TProperty>(
#if !NETCOREAPP
            HtmlHelper<TModel> htmlHelper
#else
            IHtmlHelper<TModel> htmlHelper
#endif

            , Expression<Func<TModel, TProperty>> expression)
        {
#if !NETCOREAPP
            return System.Web.Mvc.ExpressionHelper.GetExpressionText(expression);
#elif !NETCOREAPP3_0
            return Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ExpressionHelper.GetExpressionText(expression);
#else
            var expresionProvider = htmlHelper.ViewContext.HttpContext.RequestServices.GetService(typeof(ModelExpressionProvider)) as ModelExpressionProvider;
            return expresionProvider.GetExpressionText(expression);
#endif
        }
    }
}
