// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETCOREAPP
using Fireasy.Common.Logging;
using Fireasy.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Fireasy.Common.Extensions;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 负责控制器的操作方法。该类在执行期间附加了一个 <see cref="ActionContext"/> 实例，以及对 <see cref="JsonResult"/> 类型的返回结果进行处理。
    /// </summary>
    public class ControllerActionInvoker : System.Web.Mvc.ControllerActionInvoker
    {
        internal static ControllerActionInvoker Instance = new ControllerActionInvoker();

        /// <summary>
        /// 获取参数的值。
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="parameterDescriptor"></param>
        /// <returns></returns>
        protected override object GetParameterValue(ControllerContext controllerContext, ParameterDescriptor parameterDescriptor)
        {
            var type = parameterDescriptor.ParameterType.GetNonNullableType();
            if (type.IsValueType || type.IsEnum || type == typeof(string))
            {
                var value = controllerContext.HttpContext.Request.Params[parameterDescriptor.ParameterName];
                if (value == "null")
                {
                    return null;
                }

                return base.GetParameterValue(controllerContext, parameterDescriptor);
            }
            else 
            {
                //对json进行反序列化，由于使用了基于 Fireasy AOP 的实体模型，所以必须使用 Fireasy 的反序列化方法 
                var json = controllerContext.HttpContext.Request.Params[parameterDescriptor.ParameterName];
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        var option = new JsonSerializeOption();

                        var globalconverters = GlobalSetting.Converters.Where(s => s is JsonConverter).Cast<JsonConverter>();
                        option.Converters.AddRange(globalconverters);

                        var serializer = new JsonSerializer(option);
                        return serializer.Deserialize(json, parameterDescriptor.ParameterType);
                    }
                    catch (Exception exp)
                    {
                        var logger = LoggerFactory.CreateLogger();
                        if (logger != null)
                        {
                            var message = string.Format("无法解析控制器 {0} 的方法 {1} 的参数 {2} 的值。\n\n数据为: {3}", 
                                parameterDescriptor.ActionDescriptor.ControllerDescriptor.ControllerName, 
                                parameterDescriptor.ActionDescriptor.ActionName, 
                                parameterDescriptor.ParameterName,
                                json);

                            logger.Error(message, exp);
                        }
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// 执行控制器的动作。
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public override bool InvokeAction(ControllerContext controllerContext, string actionName)
        {
            using (var scope = new ActionContext(controllerContext))
            {
                return base.InvokeAction(controllerContext, actionName);
            }
        }

        /// <summary>
        /// 执行动作方法。
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="actionDescriptor"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected override ActionResult InvokeActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
        {
            if (ActionContext.Current != null)
            {
                ActionContext.Current.ActionDescriptor = actionDescriptor;
                ActionContext.Current.Parameters = parameters;
            }

            var result = base.InvokeActionMethod(controllerContext, actionDescriptor, parameters);
            var jsonResult = result as JsonResult;
            if (jsonResult != null && !(result is JsonResultWrapper))
            {
                return WrapJsonResult(jsonResult);
            }

            return result;
        }

        /// <summary>
        /// 对 <see cref="JsonResult"/> 对象进行包装并转换输出结果。
        /// </summary>
        /// <param name="jsonResult"></param>
        /// <returns></returns>
        protected virtual ActionResult WrapJsonResult(JsonResult jsonResult)
        {
            return new JsonResultWrapper(jsonResult);
        }
    }
}
#endif