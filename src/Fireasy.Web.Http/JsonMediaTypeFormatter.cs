// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Fireasy.Web.Http
{
    /// <summary>
    /// Json 媒体的格式化转换器。
    /// </summary>
    public class JsonMediaTypeFormatter : System.Net.Http.Formatting.JsonMediaTypeFormatter
    {
        /// <summary>
        /// 将对象写到流中，即对对象进行 Json 序列化。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="writeStream"></param>
        /// <param name="content"></param>
        /// <param name="transportContext"></param>
        /// <returns></returns>
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            try
            {
                var option = new JsonSerializeOption();
                option.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss"));
                var serializer = new JsonSerializer(option);
                var streamWriter = new StreamWriter(writeStream, this.SupportedEncodings.First());
                var json = serializer.Serialize(value);
                streamWriter.Write(json);
                streamWriter.Flush();

                return Task.FromResult(new AsyncVoid());
            }
            catch (Exception exception)
            {
                TaskCompletionSource<AsyncVoid> source = new TaskCompletionSource<AsyncVoid>();
                source.SetException(exception);
                return source.Task;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct AsyncVoid
        {
        }
    }
}
