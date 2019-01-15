#if NETSTANDARD
using Fireasy.Common.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Fireasy.Web.Mvc
{
    public class JsonModelBinderProvider : IModelBinderProvider
    {
        private MvcOptions options;

        public JsonModelBinderProvider(MvcOptions options)
        {
            this.options = options;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            var type = context.Metadata.ModelType.GetNonNullableType();

            if (!type.IsValueType && !type.IsEnum && type != typeof(string))
            {
                return new JsonModelBinder(options);
            }

            return null;
        }
    }
}
#endif
