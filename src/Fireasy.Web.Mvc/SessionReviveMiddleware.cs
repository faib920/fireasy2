// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETCOREAPP

using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 在已认证用户中提供 Session 复活后的通知处理。
    /// </summary>
    public class SessionReviveMiddleware
    {
        private RequestDelegate next;
        private ISessionReviveNotification notification;

        /// <summary>
        /// 初始化 <see cref="SessionReviveMiddleware"/> 类的新实例。
        /// </summary>
        /// <param name="next"></param>
        /// <param name="notification"></param>
        public SessionReviveMiddleware(RequestDelegate next, ISessionReviveNotification notification)
        {
            this.next = next;
            this.notification = notification;
        }

        public async Task Invoke(HttpContext context)
        {
            if (notification != null &&
                context.Session != null &&
                context.Session.Keys.Count() == 0 &&
                context.User.Identity.IsAuthenticated)
            {
                notification.Invoke(context);
            }

            await next(context);
        }
    }
}
#endif