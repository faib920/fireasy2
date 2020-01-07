// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Microsoft.AspNetCore.Http;
#endif
using System.Net.WebSockets;
using System.Security.Principal;

namespace Fireasy.Web.Sockets
{
    public class WebSocketAcceptContext
    {
        public WebSocket WebSocket { get; }

#if NETSTANDARD
        public HttpContext HttpContext { get; }
#endif

        public IPrincipal User { get; }

        public WebSocketBuildOption Option { get; }

        public string ConnectionId { get; internal set; }

#if NETSTANDARD
        public WebSocketAcceptContext(HttpContext context, WebSocket webSocket, IPrincipal user, WebSocketBuildOption option)
        {
            HttpContext = context;
#else
        public WebSocketAcceptContext(WebSocket webSocket, IPrincipal user, WebSocketBuildOption option)
        {
#endif
            WebSocket = webSocket;
            User = user;
            Option = option;
        }
    }
}
