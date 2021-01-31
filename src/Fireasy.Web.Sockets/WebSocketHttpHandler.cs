﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETSTANDARD
using Fireasy.Common.Ioc;
using System.Web;

namespace Fireasy.Web.Sockets
{
    public class WebSocketHttpHandler : IHttpHandler
    {
        bool IHttpHandler.IsReusable => false;

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
            {
                context.AcceptWebSocketRequest(async c =>
                {
                    var handlerType = WebSocketBuildOption.Default.GetHandlerType(context.Request.Path);
                    using (var scope = ContainerUnity.GetContainer().CreateScope())
                    {
                        var handler = HandlerCreator.CreateHandler(scope.ServiceProvider, WebSocketBuildOption.Default, handlerType);
                        if (handler != null)
                        {
                            var acceptContext = new WebSocketAcceptContext(scope.ServiceProvider, c.WebSocket, context.User, WebSocketBuildOption.Default);
                            await WebSocketHandler.Accept(handlerType, acceptContext);
                        }
                    }
                });
            }
        }
    }
}
#endif