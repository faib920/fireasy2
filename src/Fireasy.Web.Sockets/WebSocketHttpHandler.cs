// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETCOREAPP
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
                    if (handlerType != null && typeof(WebSocketHandler).IsAssignableFrom(handlerType))
                    {
                        var acceptContext = new WebSocketAcceptContext(c.WebSocket, context.User, WebSocketBuildOption.Default);
                        await WebSocketHandler.Accept(handlerType, acceptContext);
                    }
                });
            }
        }
    }
}
#endif