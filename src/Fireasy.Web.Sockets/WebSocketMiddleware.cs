// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
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
        private RequestDelegate next;
        private WebSocketBuildOption option;
        private IServiceProvider serviceProvider;

        /// <summary>
        /// 初始化 <see cref="SessionReviveMiddleware"/> 类的新实例。
        /// </summary>
        /// <param name="next"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="option"></param>
        public WebSocketMiddleware(RequestDelegate next, IServiceProvider serviceProvider, WebSocketBuildOption option)
        {
            this.next = next;
            this.serviceProvider = serviceProvider;
            this.option = option;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var handlerType = option.GetHandlerType(context.Request.Path);

                if (handlerType != null && typeof(WebSocketHandler).IsAssignableFrom(handlerType))
                {
                    var handler = GetHandler(handlerType);
                    if (handler != null)
                    {
                        var acceptContext = new WebSocketAcceptContext(webSocket, context.User, option);
                        await WebSocketHandler.Accept(handler, acceptContext);
                    }
                }
            }
            else
            {
                await next(context);
            }
        }

        private WebSocketHandler GetHandler(Type handlerType)
        {
            var constructor = handlerType.GetConstructors().FirstOrDefault();
            if (constructor == null)
            {
                throw new Exception($"No default constructor of {handlerType}.");
            }

            var parameters = constructor.GetParameters();
            var arguments = new object[parameters.Length];

            if (serviceProvider != null)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    arguments[i] = serviceProvider.GetService(parameters[i].ParameterType);
                }
            }

            return (WebSocketHandler)constructor.Invoke(arguments);
        }
    }
}
#endif
