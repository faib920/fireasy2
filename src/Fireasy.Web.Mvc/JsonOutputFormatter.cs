// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD2_0
using Fireasy.Common.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Linq;
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

        public JsonOutputFormatter()
        {
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.TextJson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationAnyJsonSyntax);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;

            using (var writer = context.WriterFactory(response.Body, selectedEncoding))
            {
                JsonSerializeOption option = null;
                if (context.Object is JsonResultWrapper wrapper)
                {
                    option = wrapper.Option;
                }

                option = option ?? new JsonSerializeOption();

                var globalconverters = GlobalSetting.Converters.Where(s => s is JsonConverter).Cast<JsonConverter>();
                option.Converters.AddRange(globalconverters);

                var serializer = new JsonSerializer(option);
                using (var jsonWriter = new JsonWriter(writer))
                {
                    serializer.Serialize(context.Object, jsonWriter);
                }
            }

            return Task.CompletedTask;
        }
    }
}
#endif