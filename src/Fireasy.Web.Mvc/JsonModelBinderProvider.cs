﻿#if NETCOREAPP
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Fireasy.Web.Mvc
{
    public class JsonModelBinderProvider : IModelBinderProvider
    {
        private readonly MvcOptions _options;

        public JsonModelBinderProvider(MvcOptions options)
        {
            _options = options;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if ((context.BindingInfo.BindingSource != null &&
                context.BindingInfo.BindingSource.Id == "Services") || !context.Metadata.IsComplexType)
            {
                return null;
            }

            return new JsonModelBinder(_options);
        }
    }
}
#endif
