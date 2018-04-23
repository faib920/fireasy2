// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD2_0
using Fireasy.Common.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Web.Mvc
{
    public class JsonResultExecutor : Microsoft.AspNetCore.Mvc.Formatters.Json.Internal.JsonResultExecutor
    {
        private static readonly string DefaultContentType = new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/json")
        {
            Encoding = Encoding.UTF8
        }.ToString();

        public JsonResultExecutor(IHttpResponseStreamWriterFactory writerFactory, ILogger<JsonResultExecutor> logger, IOptions<MvcJsonOptions> options, ArrayPool<char> charPool)
            : base(writerFactory, logger, options, charPool)
        {
        }

        public override Task ExecuteAsync(Microsoft.AspNetCore.Mvc.ActionContext context, JsonResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var response = context.HttpContext.Response;

            ResponseContentTypeHelper.ResolveContentTypeAndEncoding(
                result.ContentType,
                response.ContentType,
                DefaultContentType,
                out var resolvedContentType,
                out var resolvedContentTypeEncoding);

            response.ContentType = resolvedContentType;

            if (result.Value != null)
            {
                using (var writer = WriterFactory.CreateWriter(response.Body, resolvedContentTypeEncoding))
                {
                    JsonSerializeOption option = null;
                    if (result is JsonResultWrapper wrapper)
                    {
                        option = wrapper.Option;
                    }

                    option = option ?? new JsonSerializeOption();

                    var globalconverters = GlobalSetting.Converters.Where(s => s is JsonConverter).Cast<JsonConverter>();
                    option.Converters.AddRange(globalconverters);

                    var serializer = new JsonSerializer(option);
                    using (var jsonWriter = new JsonWriter(writer))
                    {
                        serializer.Serialize(result.Value, jsonWriter);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
#endif
