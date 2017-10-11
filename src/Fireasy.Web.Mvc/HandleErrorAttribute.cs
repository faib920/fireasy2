// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Logging;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 控制器方法执行发生异常时，记录日志并返回友好的提示信息。
    /// </summary>
    public class HandleErrorAttribute : System.Web.Mvc.HandleErrorAttribute
    {
        /// <summary>
        /// 处理异常信息。
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(ExceptionContext filterContext)
        {
            if (IsJsonResult(filterContext))
            {
                HandleExceptionForJson(filterContext);
            }
            else
            {
                HandleException(filterContext);
            }

            LogException(filterContext);
        }

        /// <summary>
        /// 判断返回结果是否为 Json 类型。
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        protected virtual bool IsJsonResult(ExceptionContext filterContext)
        {
            if (ActionContext.Current != null)
            {
                var desc = ActionContext.Current.ActionDescriptor as ReflectedActionDescriptor;
                if (desc != null)
                {
                    return typeof(JsonResult).IsAssignableFrom(desc.MethodInfo.ReturnType);
                }
            }

            return false;
        }

        /// <summary>
        /// 处理返回结果为Json的异常信息。
        /// </summary>
        /// <param name="filterContext"></param>
        protected virtual void HandleExceptionForJson(ExceptionContext filterContext)
        {
            //如果是通知类的异常，直接输出提示
            var notifyExp = filterContext.Exception as Fireasy.Common.ClientNotificationException;
            if (notifyExp != null)
            {
                filterContext.Result = new JsonResultWrapper(new JsonResult { Data = Result.Fail(notifyExp.Message) });
                filterContext.ExceptionHandled = true;
                return;
            }
            else
            {
                filterContext.Result = GetHandledResult();
                filterContext.ExceptionHandled = true;
            }
        }

        /// <summary>
        /// 处理一般返回结果的异常信息。
        /// </summary>
        /// <param name="filterContext"></param>
        protected virtual void HandleException(ExceptionContext filterContext)
        {
            var errorPage = ConfigurationManager.AppSettings["error-page"];
            if (!string.IsNullOrEmpty(errorPage))
            {
                filterContext.Result = new RedirectResult(errorPage);
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
            var logger = LoggerFactory.CreateLogger();
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
        /// <returns></returns>
        protected virtual ActionResult GetHandledResult()
        {
            if (ActionContext.Current != null)
            {
                //检查是否定义了 ExceptionBehaviorAttribute 特性
                var attr = ActionContext.Current.ActionDescriptor
                    .GetCustomAttributes(typeof(EmptyArrayResultAttribute), false)
                    .Cast<EmptyArrayResultAttribute>().FirstOrDefault();

                if (attr != null)
                {
                    //返回空数组，一般用在列表绑定上
                    if (attr.EmptyArray)
                    {
                        return new JsonResultWrapper(new JsonResult { Data = new string[0] });
                    }
                    //使用提示信息
                    else if (!string.IsNullOrEmpty(attr.Message))
                    {
                        return new JsonResultWrapper(new JsonResult { Data = Result.Fail(attr.Message) });
                    }
                }
            }

            return new JsonResultWrapper(new JsonResult { Data = Result.Fail("发生错误，请查阅相关日志或联系管理员。") });
        }
    }
}
