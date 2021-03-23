#if NETCOREAPP
using Fireasy.Common.Extensions;
using Fireasy.Common.Logging;
using Fireasy.Common.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 数据绑定器，以 Json 字符串的方式进行绑定。
    /// </summary>
    public class JsonModelBinder : IModelBinder
    {
        private readonly MvcOptions _mvcOptions;

        public JsonModelBinder(MvcOptions mvcOptions)
        {
            _mvcOptions = mvcOptions;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = await GetValueAsync(bindingContext);
            if (value == ValueProviderResult.None)
            {
                if (bindingContext.Model != null)
                {
                    bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
                }

                return;
            }

            var serviceProvider = bindingContext.HttpContext.RequestServices;

            var modelState = bindingContext.ModelState;
            modelState.SetModelValue(bindingContext.ModelName, value);

            var serializer = serviceProvider.TryGetService<ISerializer>(() => new JsonSerializer());
            serializer.Option = new JsonSerializeOption(_mvcOptions.JsonSerializeOption);

            try
            {
                object obj;
                if (serializer is ITextSerializer txtSerializer)
                {
                    obj = txtSerializer.Deserialize(value.FirstValue, bindingContext.ModelType);
                }
                else
                {
                    obj = serializer.Deserialize(Encoding.UTF8.GetBytes(value.FirstValue), bindingContext.ModelType);
                }

                bindingContext.Result = ModelBindingResult.Success(obj);
            }
            catch (Exception exp)
            {
                var logger = LoggerFactory.CreateLogger();
                if (logger != null)
                {
                    var message = string.Format("无法解析控制器 {0} 的方法 {1} 的参数 {2} 的值。\n\n数据为: {3}",
                        bindingContext.ActionContext.RouteData.Values["controller"],
                        bindingContext.ActionContext.RouteData.Values["action"],
                        bindingContext.ModelName,
                        value.FirstValue);

                    logger.Error(message, exp);
                }

                bindingContext.Result = ModelBindingResult.Failed();
            }
        }

        private async Task<ValueProviderResult> GetValueAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.BindingSource?.CanAcceptDataFrom(BindingSource.Body) == true)
            {
                var body = bindingContext.HttpContext.Request.Body;
                using var reader = new StreamReader(body, Encoding.UTF8);
                var content = await reader.ReadToEndAsync();
                return new ValueProviderResult(new StringValues(content));
            }
            else if (!string.IsNullOrEmpty(bindingContext.ModelName))
            {
                return bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            }

            return ValueProviderResult.None;
        }
    }
}
#endif