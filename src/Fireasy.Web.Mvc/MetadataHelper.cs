using System.Linq.Expressions;

namespace Fireasy.Web.Mvc
{
    public static class MetadataHelper
    {
        public static string GetPropertyName(LambdaExpression expression)
        {
#if !NETCOREAPP
            return System.Web.Mvc.ExpressionHelper.GetExpressionText(expression);
#else
            return Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ExpressionHelper.GetExpressionText(expression);
#endif
        }
    }
}
