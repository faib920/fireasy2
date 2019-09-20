// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;
#if !NETCOREAPP
using System;
using System.Linq;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc;
#endif
namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 对 <see cref="JsonResult"/> 进行包装，重写序列化对象的方法。
    /// </summary>
    public class JsonResultWrapper : JsonResult
    {
        /// <summary>
        /// 初始化 <see cref="JsonResultWrapper"/> 类的新实例。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="option"></param>
        public JsonResultWrapper(object value, JsonSerializeOption option = null)
#if NETCOREAPP
            : base(value)
#endif
        {
#if !NETCOREAPP
            this.result = new JsonResult { Data = value, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
#endif
            Option = option;
        }

        public JsonSerializeOption Option { get; set; }

#if !NETCOREAPP
        private JsonResult result;

        /// <summary>
        /// 初始化 <see cref="JsonResultWrapper"/> 类的新实例。
        /// </summary>
        /// <param name="result"></param>
        /// <param name="behavior"></param>
        public JsonResultWrapper(JsonResult result, JsonRequestBehavior behavior = JsonRequestBehavior.AllowGet, JsonSerializeOption option = null)
        {
            this.result = result;
            this.result.JsonRequestBehavior = behavior;
            Option = option;
        }

        /// <summary>
        /// 将结果输出到 Response。
        /// </summary>
        /// <param name="context"></param>
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if ((result.JsonRequestBehavior == JsonRequestBehavior.DenyGet) && 
                string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("此请求已被阻止，因为当用在 GET 请求中时，会将敏感信息透漏给第三方网站。若要允许 GET 请求，请将 JsonRequestBehavior 设置为 AllowGet。");
            }

            var response = context.HttpContext.Response;
            if (!string.IsNullOrEmpty(result.ContentType))
            {
                response.ContentType = result.ContentType;
            }
            else
            {
                response.ContentType = "application/json";
            }

            if (result.ContentEncoding != null)
            {
                response.ContentEncoding = result.ContentEncoding;
            }

            if (result.Data != null)
            {
                response.Write(SerializeJson(context, result.Data));
            }
        }

        /// <summary>
        /// 将数据序列化为 Json 字符串。这里使用了 Fireasy 提供的 Json 序列化方法。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="data">要序列化的数据。</param>
        /// <returns></returns>
        protected virtual string SerializeJson(ControllerContext context, object data)
        {
            var option = Option ?? new JsonSerializeOption();

            var globalconverters = GlobalSetting.Converters.Where(s => s is JsonConverter).Cast<JsonConverter>();
            option.Converters.AddRange(globalconverters);

            if (ActionContext.Current != null)
            {
                var scopeConverters = ActionContext.Current.Converters.Where(s => s is JsonConverter).Cast<JsonConverter>();
                option.Converters.AddRange(scopeConverters);
            }

            //jsonp的处理
            var jsoncallback = context.HttpContext.Request.Form["callback"];

            var serializer = new JsonSerializer(option);
            var json = serializer.Serialize(data);

            if (!string.IsNullOrEmpty(jsoncallback))
            {
                return string.Format("{0}({1})", jsoncallback, json);
            }

            return json;
        }
#endif
    }
}
