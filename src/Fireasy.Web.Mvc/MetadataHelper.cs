using System.Linq.Expressions;

namespace Fireasy.Web.Mvc
{
    public static class MetadataHelper
    {
        public static string GetPropertyName(LambdaExpression expression)
        {
#if !NETCOREAPP
            return System.Web.Mvc.ExpressionHelper.GetExpressionText(expression);
#elif !NETCOREAPP3_0
            return Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ExpressionHelper.GetExpressionText(expression);
#else
            return string.Empty;
#endif
        }
    }
}
