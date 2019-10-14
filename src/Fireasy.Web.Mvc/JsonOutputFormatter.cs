// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETCOREAPP
using Fireasy.Common.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Web.Mvc
{
    public class JsonOutputFormatter : TextOutputFormatter
    {
        class MediaTypeHeaderValues
        {
            public static readonly MediaTypeHeaderValue ApplicationJson
                = MediaTypeHeaderValue.Parse("application/json").CopyAsReadOnly();

            public static readonly MediaTypeHeaderValue TextJson
                = MediaTypeHeaderValue.Parse("text/json").CopyAsReadOnly();

            public static readonly MediaTypeHeaderValue ApplicationJsonPatch
                = MediaTypeHeaderValue.Parse("application/json-patch+json").CopyAsReadOnly();

            public static readonly MediaTypeHeaderValue ApplicationAnyJsonSyntax
                = MediaTypeHeaderValue.Parse("application/*+json").CopyAsReadOnly();
        }

        private MvcOptions mvcOptions;

        public JsonOutputFormatter(MvcOptions mvcOptions)
        {
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.TextJson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationAnyJsonSyntax);

            this.mvcOptions = mvcOptions;
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;

            JsonSerializeOption option = null;
            if (context.Object is JsonResultWrapper wrapper)
            {
                option = wrapper.Option;
            }
            else
            {
                var hosting = context.HttpContext.RequestServices.GetService<JsonSerializeOptionHosting>();
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

            var content = serializer.Serialize(context.Object);
            response.Body.WriteAsync(selectedEncoding.GetBytes(content));

            return Task.CompletedTask;
        }
    }
}
#endif