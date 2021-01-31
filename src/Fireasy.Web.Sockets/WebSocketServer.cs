// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETFRAMEWORK
using Fireasy.Common.Ioc;
using System.Net;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 基于 Http 的 WebSocket 服务。
    /// </summary>
    public class WebSocketServer
    {
        public async Task Start<T>(string uri, WebSocketBuildOption option) where T : WebSocketHandler, new()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(uri);
            listener.Start();

            while (true)
            {
                var listenerContext = await listener.GetContextAsync();
                if (listenerContext.Request.IsWebSocketRequest)
                {
                    var socketContext = await listenerContext.AcceptWebSocketAsync(null, option.KeepAliveInterval);
                    using (var scope = ContainerUnity.GetContainer().CreateScope())
                    {
                        var handler = HandlerCreator.CreateHandler(scope.ServiceProvider, option, typeof(T));
                        if (handler != null)
                        {
                            var acceptContext = new WebSocketAcceptContext(scope.ServiceProvider, socketContext.WebSocket, listenerContext.User, option);
                            await WebSocketHandler.Accept(handler, acceptContext);
                        }
                    }
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
#endif