#if NETCOREAPP3_0
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System;
using System.Text;

namespace Fireasy.Web.Mvc
{
    public class JsonHelper : IJsonHelper
    {
        private readonly HttpContext context;
        private readonly MvcOptions mvcOptions;

        public JsonHelper(IHttpContextAccessor httpAccessor, IOptions<MvcOptions> mvcOptions)
        {
            context = httpAccessor.HttpContext;
            this.mvcOptions = mvcOptions.Value;
        }

        public IHtmlContent Serialize(object value)
        {
            var serviceProvider = context.RequestServices;

            JsonSerializeOption option = null;
            if (value is JsonResultWrapper wrapper)
            {
                option = wrapper.Option;
            }
            else
            {
                if (context.RequestServices.GetService(typeof(JsonSerializeOptionHosting)) is JsonSerializeOptionHosting hosting)
                {
                    option = hosting.Option;
                }
            }

            if (option == null)
            {
                option = mvcOptions.JsonSerializeOption;
            }
            else
            {
                option.Reference(mvcOptions.JsonSerializeOption);
            }

            var serializer = serviceProvider.TryGetService<ISerializer>(() => new JsonSerializer(option));

            string json;
            if (serializer is ITextSerializer txtSerializer)
            {
                json = txtSerializer.Serialize(value);
            }
            else
            {
                json = Encoding.UTF8.GetString(serializer.Serialize(value));
            }

            return new HtmlString(json);
        }
    }
}
#endif
