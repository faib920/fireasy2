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
            if ((context.BindingInfo.BindingSource != null &&
                context.BindingInfo.BindingSource.Id != "Services") &&
                context.Metadata.IsComplexType)
            {
                return new JsonModelBinder(options);
            }

            return null;
        }
    }
}
#endif
