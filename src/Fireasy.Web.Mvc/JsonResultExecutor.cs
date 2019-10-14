// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETCOREAPP && !NETCOREAPP3_0
using Fireasy.Common.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
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

        private MvcOptions mvcOptions;

        public JsonResultExecutor(IHttpResponseStreamWriterFactory writerFactory,
            ILogger<JsonResultExecutor> logger,
            IOptions<MvcJsonOptions> jsonOptions,
            IOptions<MvcOptions> mvcOptions,
            ArrayPool<char> charPool)
            : base(writerFactory, logger, jsonOptions, charPool)
        {
            this.mvcOptions = mvcOptions.Value;
        }

        public override Task ExecuteAsync(ActionContext context, JsonResult result)
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
                JsonSerializeOption option = null;
                if (result is JsonResultWrapper wrapper)
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

                var content = serializer.Serialize(result.Value);
                response.Body.WriteAsync(resolvedContentTypeEncoding.GetBytes(content));
            }

            return Task.CompletedTask;
        }
    }
}
#elif NETCOREAPP3_0
using Fireasy.Common.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Web.Mvc
{
    public class JsonResultExecutor : IActionResultExecutor<JsonResult>
    {
        private static readonly string DefaultContentType = new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/json")
        {
            Encoding = Encoding.UTF8
        }.ToString();

        private MvcOptions mvcOptions;

        public JsonResultExecutor(IOptions<MvcOptions> mvcOptions)
        {
            this.mvcOptions = mvcOptions.Value;
        }

        public Task ExecuteAsync(ActionContext context, JsonResult result)
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

            ResolveContentTypeAndEncoding(
                result.ContentType,
                response.ContentType,
                DefaultContentType,
                out var resolvedContentType,
                out var resolvedContentTypeEncoding);

            response.ContentType = resolvedContentType;

            if (result.Value != null)
            {
                JsonSerializeOption option = null;
                if (result is JsonResultWrapper wrapper)
                {
                    option = wrapper.Option;
                }
                else
                {
                    var hosting = context.HttpContext.RequestServices.GetService(typeof(JsonSerializeOptionHosting)) as JsonSerializeOptionHosting;
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

                var content = serializer.Serialize(result.Value);
                response.Body.WriteAsync(resolvedContentTypeEncoding.GetBytes(content));
            }

            return Task.CompletedTask;
        }

        static void ResolveContentTypeAndEncoding(string actionResultContentType, string httpResponseContentType, string defaultContentType, out string resolvedContentType, out Encoding resolvedContentTypeEncoding)
        {
            Encoding encoding = MediaType.GetEncoding(defaultContentType);
            if (actionResultContentType != null)
            {
                resolvedContentType = actionResultContentType;
                Encoding encoding2 = MediaType.GetEncoding(actionResultContentType);
                resolvedContentTypeEncoding = (encoding2 ?? encoding);
            }
            else if (!string.IsNullOrEmpty(httpResponseContentType))
            {
                Encoding encoding3 = MediaType.GetEncoding(httpResponseContentType);
                if (encoding3 != null)
                {
                    resolvedContentType = httpResponseContentType;
                    resolvedContentTypeEncoding = encoding3;
                }
                else
                {
                    resolvedContentType = httpResponseContentType;
                    resolvedContentTypeEncoding = encoding;
                }
            }
            else
            {
                resolvedContentType = defaultContentType;
                resolvedContentTypeEncoding = encoding;
            }
        }
    }
}
#endif
