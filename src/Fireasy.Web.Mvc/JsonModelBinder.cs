#if NETSTANDARD2_0
using Fireasy.Common.Logging;
using Fireasy.Common.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 数据绑定器，以 Json 字符串的方式进行绑定。
    /// </summary>
    public class JsonModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (value == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            var modelState = bindingContext.ModelState;
            modelState.SetModelValue(bindingContext.ModelName, value);

            var option = new JsonSerializeOption();

            var globalconverters = GlobalSetting.Converters.Where(s => s is JsonConverter).Cast<JsonConverter>();
            option.Converters.AddRange(globalconverters);

            var serializer = new JsonSerializer(option);

            try
            {
                var json = serializer.Deserialize(value.FirstValue, bindingContext.ModelType);
                bindingContext.Result = ModelBindingResult.Success(json);
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

            return Task.CompletedTask;
        }
    }
}
#endif