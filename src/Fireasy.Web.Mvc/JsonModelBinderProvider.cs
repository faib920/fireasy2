#if NETSTANDARD2_0
using Fireasy.Common.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Fireasy.Web.Mvc
{
    public class JsonModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            var type = context.Metadata.ModelType.GetNonNullableType();

            if (!type.IsValueType && !type.IsEnum && type != typeof(string))
            {
                return new JsonModelBinder();
            }

            return null;
        }
    }
}
#endif
