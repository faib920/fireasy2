// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Net;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 基于 Http 的 WebSocket 服务。
    /// </summary>
    public class WebSocketServer
    {
        public async Task Start<T>(string uri) where T : WebSocketHandler, new()
        {
            await Start<T>(uri, TimeSpan.FromMinutes(6), TimeSpan.FromSeconds(30));
        }

        public async Task Start<T>(string uri, TimeSpan keepAliveInterval, TimeSpan heartbeatInterval, int heartbeatTryTimes = 3) where T : WebSocketHandler, new()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(uri);
            listener.Start();

            while (true)
            {
                var listenerContext = await listener.GetContextAsync();
                if (listenerContext.Request.IsWebSocketRequest)
                {
                    var socketContext = await listenerContext.AcceptWebSocketAsync(null, keepAliveInterval);
                    var acceptContext = new WebSocketAcceptContext(socketContext.WebSocket, listenerContext.User, heartbeatInterval, heartbeatTryTimes);
                    await WebSocketHandler.Accept<T>(acceptContext);
                }
                else
                {
                    listenerContext.Response.StatusCode = 404;
                    listenerContext.Response.Close();
                }
            }
        }
    }
}
