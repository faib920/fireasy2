// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETCOREAPP

using Fireasy.Common.Extensions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 在已认证用户中提供 Session 复活后的通知处理。
    /// </summary>
    public class SessionReviveMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISessionReviveNotification _notification;

        /// <summary>
        /// 初始化 <see cref="SessionReviveMiddleware"/> 类的新实例。
        /// </summary>
        /// <param name="next"></param>
        /// <param name="notification"></param>
        public SessionReviveMiddleware(RequestDelegate next, ISessionReviveNotification notification)
        {
            _next = next;
            _notification = notification;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_notification != null &&
                context.Session != null &&
                context.Session.Keys.IsNullOrEmpty() &&
                context.User.Identity.IsAuthenticated)
            {
                await _notification.InvokeAsync(context);
            }

            await _next(context);
        }
    }
}
#endif