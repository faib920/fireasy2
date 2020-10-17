// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using Fireasy.Common.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
#if !NETCOREAPP
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 控制器方法执行发生异常时，记录日志并返回友好的提示信息。
    /// </summary>
    public class HandleErrorAttribute :
#if !NETCOREAPP
        System.Web.Mvc.HandleErrorAttribute
#else
        ExceptionFilterAttribute
#endif
    {
        /// <summary>
        /// 处理异常信息。
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(ExceptionContext filterContext)
        {
#if !NETCOREAPP
            var descriptor = ActionContext.Current != null ? ActionContext.Current.ActionDescriptor as ReflectedActionDescriptor : null;
            if (descriptor != null && IsJsonResult(descriptor.MethodInfo.ReturnType))
#else
            var descriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor != null && IsJsonResult(descriptor.MethodInfo.ReturnType))
#endif
            {
                HandleExceptionForJson(filterContext);
            }
            else if (descriptor != null &&
                descriptor.MethodInfo.GetCustomAttributes<ActionJsonResultAttribute>().Any())
            {
                HandleExceptionForJson(filterContext);
            }

            LogException(filterContext);
        }

        /// <summary>
        /// 处理返回结果为Json的异常信息。
        /// </summary>
        /// <param name="filterContext"></param>
        protected virtual void HandleExceptionForJson(ExceptionContext filterContext)
        {
            ActionResult result;
            //如果是通知类的异常，直接输出提示
            var notifyExp = GetNotificationException(filterContext.Exception);
            if (notifyExp != null)
            {
                result = new JsonResultWrapper(Result.Fail(notifyExp.Message));
            }
            else
            {
                result = GetHandledResult(filterContext);
            }

            if (result != null)
            {
                filterContext.Result = result;
                filterContext.ExceptionHandled = true;
            }

        }

        /// <summary>
        /// 记录异常日志。
        /// </summary>
        /// <param name="filterContext"></param>
        protected virtual void LogException(ExceptionContext filterContext)
        {
            var controllerName = (string)filterContext.RouteData.Values["controller"];
            var actionName = (string)filterContext.RouteData.Values["action"];

            Tracer.Error($"Throw exception when '{controllerName}.{actionName}' is executed:\n{filterContext.Exception.Output()}");

            //记录日志
#if !NETCOREAPP
            var logger = LoggerFactory.CreateLogger();
#else
            var logger = ServiceProviderServiceExtensions.GetService<ILogger>(filterContext.HttpContext.RequestServices);
#endif
            if (logger != null)
            {

                logger.Error(string.Format("执行控制器 {0} 的方法 {1} 时发生错误。",
                    controllerName, actionName), filterContext.Exception);
            }
        }

        /// <summary>
        /// 获取处理后的返回结果。
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        protected virtual ActionResult GetHandledResult(ExceptionContext filterContext)
        {
            EmptyArrayResultAttribute attr = null;
#if !NETCOREAPP
            var serviceProvider = ContainerUnity.GetContainer();
            if (ActionContext.Current != null)
            {
                attr = ActionContext.Current.ActionDescriptor
                    .GetCustomAttributes<EmptyArrayResultAttribute>().FirstOrDefault();

            }
#else
            var serviceProvider = filterContext.HttpContext.RequestServices;
            var descriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor != null)
            {
                attr = descriptor.MethodInfo
                    .GetCustomAttributes(typeof(EmptyArrayResultAttribute), false)
                    .Cast<EmptyArrayResultAttribute>().FirstOrDefault();
            }
#endif
            if (attr != null)
            {
                //返回空数组，一般用在列表绑定上
                if (attr.EmptyArray)
                {
                    return new JsonResultWrapper(new string[0]);
                }
                //使用提示信息
                else if (!string.IsNullOrEmpty(attr.Message))
                {
                    return new JsonResultWrapper(Result.Fail(attr.Message));
                }
            }

            var handler = serviceProvider.TryGetService<IExceptionHandler>();
            if (handler != null)
            {
                return handler.GetResult(filterContext.Exception);
            }

            return new JsonResultWrapper(Result.Fail("发生错误，请查阅相关日志或联系管理员。"));
        }

        /// <summary>
        /// 判断是否为 <see cref="JsonResult"/> 类型。
        /// </summary>
        /// <param name="returnType"></param>
        /// <returns></returns>
        private bool IsJsonResult(Type returnType)
        {
            return typeof(JsonResult).IsAssignableFrom(returnType) ||
                (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>) && typeof(JsonResult).IsAssignableFrom(returnType.GetGenericArguments()[0]));
        }

        /// <summary>
        /// 查找 <see cref="ClientNotificationException"/> 异常。
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        private ClientNotificationException GetNotificationException(Exception exp)
        {
            while (exp != null)
            {
                if (exp is ClientNotificationException notifyExp)
                {
                    return notifyExp;
                }

                exp = exp.InnerException;
            }

            return null;
        }
    }
}