// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// WebSocket 中间件。
    /// </summary>
    public class WebSocketMiddleware
    {
        private RequestDelegate next;
        private WebSocketBuildOption options;

        /// <summary>
        /// 初始化 <see cref="SessionReviveMiddleware"/> 类的新实例。
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        public WebSocketMiddleware(RequestDelegate next, WebSocketBuildOption options)
        {
            this.next = next;
            this.options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var handlerType = options.GetHandlerType(context.Request.Path);

                if (handlerType == null || !typeof(WebSocketHandler).IsAssignableFrom(handlerType))
                {
                    context.Response.StatusCode = 400;
                }
                else
                {
                    await WebSocketHandler.Accept(handlerType, context.User, webSocket);
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}
