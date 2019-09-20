// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Logging;
using System;
using System.Linq;
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
            if (descriptor != null && typeof(JsonResult).IsAssignableFrom(descriptor.MethodInfo.ReturnType))
#else
            var descriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor != null && typeof(JsonResult).IsAssignableFrom(descriptor.MethodInfo.ReturnType))
#endif
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
            //如果是通知类的异常，直接输出提示
            var notifyExp = GetNotificationException(filterContext.Exception);
            if (notifyExp != null)
            {
                filterContext.Result = new JsonResultWrapper(Result.Fail(notifyExp.Message));
                filterContext.ExceptionHandled = true;
                return;
            }
            else
            {
                filterContext.Result = GetHandledResult(filterContext);
                filterContext.ExceptionHandled = true;
            }
        }

        /// <summary>
        /// 记录异常日志。
        /// </summary>
        /// <param name="filterContext"></param>
        protected virtual void LogException(ExceptionContext filterContext)
        {
            //记录日志
#if !NETCOREAPP
            var logger = LoggerFactory.CreateLogger();
#else
            var logger = filterContext.HttpContext.RequestServices.GetService<ILogger>();
#endif
            if (logger != null)
            {
                var controllerName = (string)filterContext.RouteData.Values["controller"];
                var actionName = (string)filterContext.RouteData.Values["action"];

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
            if (ActionContext.Current != null)
            {
                attr = ActionContext.Current.ActionDescriptor
                    .GetCustomAttributes(typeof(EmptyArrayResultAttribute), false)
                    .Cast<EmptyArrayResultAttribute>().FirstOrDefault();

            }
#else
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

            return new JsonResultWrapper(Result.Fail("发生错误，请查阅相关日志或联系管理员。"));
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