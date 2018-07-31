// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Net.WebSockets;
using System.Security.Principal;

namespace Fireasy.Web.Sockets
{
    public class WebSocketAcceptContext
    {
        public TimeSpan HeartbeatInterval { get; }

        public int HeartbeatTryTimes { get; }

        public WebSocket WebSocket { get; }

        public IPrincipal User { get; }

        public WebSocketAcceptContext(WebSocket webSocket, IPrincipal user, TimeSpan heartbeatInterval, int heartbeatTryTimes)
        {
            WebSocket = webSocket;
            User = user;
            HeartbeatInterval = heartbeatInterval;
            HeartbeatTryTimes = heartbeatTryTimes;
        }
    }
}
