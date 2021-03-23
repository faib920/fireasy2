// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETCOREAPP
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
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

        private readonly MvcOptions _mvcOptions;

        public JsonOutputFormatter(MvcOptions mvcOptions)
        {
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.TextJson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationAnyJsonSyntax);

            _mvcOptions = mvcOptions;
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            var serviceProvider = context.HttpContext.RequestServices;

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
                option = new JsonSerializeOption(_mvcOptions.JsonSerializeOption);
            }
            else
            {
                option.AttachConverters(_mvcOptions.JsonSerializeOption);
            }

            var serializer = serviceProvider.TryGetService<ISerializer>(() => new JsonSerializer());
            serializer.Option = option;

            if (serializer is ITextSerializer txtSerializer)
            {
                var content = txtSerializer.Serialize(context.Object);
                response.Body.WriteAsync(selectedEncoding.GetBytes(content));
            }
            else
            {
                response.Body.WriteAsync(serializer.Serialize(context.Object));
            }

            return Task.CompletedTask;
        }
    }
}
#endif