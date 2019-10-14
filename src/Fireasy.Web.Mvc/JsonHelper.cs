#if NETCOREAPP3_0
using Fireasy.Common.Serialization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace Fireasy.Web.Mvc
{
    public class JsonHelper : IJsonHelper
    {
        private HttpContext context;
        private MvcOptions mvcOptions;

        public JsonHelper(IHttpContextAccessor httpAccessor, IOptions<MvcOptions> mvcOptions)
        {
            context = httpAccessor.HttpContext;
            this.mvcOptions = mvcOptions.Value;
        }

        public IHtmlContent Serialize(object value)
        {
            JsonSerializeOption option = null;
            if (value is JsonResultWrapper wrapper)
            {
                option = wrapper.Option;
            }
            else
            {
                var hosting = context.RequestServices.GetService(typeof(JsonSerializeOptionHosting)) as JsonSerializeOptionHosting;
                if (hosting != null)
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

            var serializer = new JsonSerializer(option);

            var json = serializer.Serialize(value);
            return new HtmlString(json);
        }
    }
}
#endif
