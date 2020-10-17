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
using System;
using System.Net.WebSockets;
using System.Security.Principal;

namespace Fireasy.Web.Sockets
{
    public class WebSocketAcceptContext
    {
        public WebSocket WebSocket { get; }

        public IServiceProvider ServiceProvider { get; }

        public IPrincipal User { get; }

        public WebSocketBuildOption Option { get; }

        public string ConnectionId { get; internal set; }

        public WebSocketAcceptContext(IServiceProvider serviceProvider, WebSocket webSocket, IPrincipal user, WebSocketBuildOption option)
        {
            ServiceProvider = serviceProvider;
            WebSocket = webSocket;
            User = user;
            Option = option;
        }
    }
}
