// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// WebSocket 中间件。
    /// </summary>
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketBuildOption _option;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 初始化 <see cref="WebSocketMiddleware"/> 类的新实例。
        /// </summary>
        /// <param name="next"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="option"></param>
        public WebSocketMiddleware(RequestDelegate next, IServiceProvider serviceProvider, WebSocketBuildOption option)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _option = option;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var handlerType = _option.GetHandlerType(context.Request.Path);

                using (var scope = context.RequestServices.CreateScope())
                {
                    var handler = HandlerCreator.CreateHandler(scope.ServiceProvider, _option, handlerType);
                    if (handler != null)
                    {
                        var acceptContext = new WebSocketAcceptContext(scope.ServiceProvider, webSocket, context.User, _option);
                        await WebSocketHandler.Accept(handler, acceptContext);
                    }
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
#endif
