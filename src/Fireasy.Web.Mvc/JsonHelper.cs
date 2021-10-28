#if NETCOREAPP3_0_OR_GREATER
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Text;

namespace Fireasy.Web.Mvc
{
    public class JsonHelper : IJsonHelper
    {
        private readonly HttpContext _context;
        private readonly MvcOptions _mvcOptions;

        public JsonHelper(IHttpContextAccessor httpAccessor, IOptions<MvcOptions> mvcOptions)
        {
            _context = httpAccessor.HttpContext;
            _mvcOptions = mvcOptions.Value;
        }

        public IHtmlContent Serialize(object value)
        {
            var serviceProvider = _context.RequestServices;

            JsonSerializeOption option = null;
            if (value is JsonResultWrapper wrapper)
            {
                option = wrapper.Option;
            }
            else
            {
                var hosting = _context.RequestServices.GetService<JsonSerializeOptionHosting>();
                option = hosting?.Option;
            }

            var serializer = serviceProvider.TryGetService<ISerializer>(() => new JsonSerializer());
            serializer.Option = option ?? new JsonSerializeOption(_mvcOptions.JsonSerializeOption);

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
