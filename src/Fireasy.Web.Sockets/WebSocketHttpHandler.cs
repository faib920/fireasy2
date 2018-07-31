// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETSTANDARD2_0
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
                    if (handlerType == null || !typeof(WebSocketHandler).IsAssignableFrom(handlerType))
                    {
                        context.Response.StatusCode = 400;
                    }
                    else
                    {
                        var acceptContext = new WebSocketAcceptContext(c.WebSocket, context.User, WebSocketBuildOption.Default.HeartbeatInterval, WebSocketBuildOption.Default.HeartbeatTryTimes);
                        await WebSocketHandler.Accept(handlerType, acceptContext);
                    }
                });
            }
        }
    }
}
#endif