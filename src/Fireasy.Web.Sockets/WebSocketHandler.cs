// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using System;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    public abstract class WebSocketHandler : IClientProxy, IDisposable
    {
        private WebSocket webSocket;

        public IPrincipal User { get; private set; }

        public string ConnectionId { get; private set; }

        public ClientManager Clients { get; private set; }

        public GroupManager Groups { get; private set; }

        public WebSocketHandler()
        {
            this.ConnectionId = Guid.NewGuid().ToString();
        }

        public static async Task Accept<T>(WebSocketContext webSocketContext) where T : WebSocketHandler, new()
        {
            await Accept<T>(webSocketContext.User, webSocketContext.WebSocket);
        }

        public static async Task Accept<T>(IPrincipal user, WebSocket webSocket) where T : WebSocketHandler, new()
        {
            await Accept(typeof(T), user, webSocket);
        }

        public static async Task Accept(Type handlerType, IPrincipal user, WebSocket webSocket)
        {
            var handler = handlerType.New<WebSocketHandler>();
            handler.User = user;
            handler.webSocket = webSocket;
            handler.Clients = ClientManager.GetManager(handlerType);
            handler.Groups = GroupManager.GetManager(handler.Clients);

            await handler.Invoke();
        }

        async Task IClientProxy.SendAsync(string method, params object[] arguments)
        {
            var option = new JsonSerializeOption() { Indent = false };
            var serializer = new JsonSerializer(option);

            var message = new InvokeMessage(method, 0, arguments);
            var json = serializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task Invoke()
        {
            OnConnected();
            Clients.Add(ConnectionId, this);

            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                var bytes = HandleResult(buffer, result.Count, s => OnReceived(s));

                if (bytes.Length > 0)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None)
                .ContinueWith(t =>
                {
                    OnDisconnected();
                    Clients.Remove(ConnectionId);
                });
        }

        protected virtual void OnConnected()
        {
        }

        protected virtual void OnDisconnected()
        {
        }

        protected virtual void OnReceived(string content)
        {
        }

        protected virtual void OnResolveError(string content, Exception exception)
        {
        }

        public void Dispose()
        {
            if (webSocket != null)
            {
                webSocket.Dispose();
            }
        }

        private byte[] HandleResult(byte[] buffer, int length, Action<string> receiver)
        {
            var serializer = new JsonSerializer();
            var content = Encoding.UTF8.GetString(buffer, 0, length);

            try
            {
                receiver?.Invoke(content);

                var obj = serializer.Deserialize<InvokeMessage>(content);

                var method = this.GetType().GetMethod(obj.Method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (method == null)
                {
                    throw new Exception($"没有发现方法 {obj.Method}");
                }

                if (method.GetParameters().Length != obj.Arguments.Length)
                {
                    throw new Exception($"方法 {obj.Method} 参数不匹配");
                }

                var result = method.Invoke(this, obj.Arguments);
                if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    result = method.ReturnType.GetProperty("Result").GetValue(result);
                }

                if (method.ReturnType != typeof(void))
                {
                    var ret = new InvokeMessage(obj.Method, 1, new[] { result });
                    return Encoding.UTF8.GetBytes(serializer.Serialize(ret));
                }
            }
            catch (Exception exp)
            {
                OnResolveError(content, exp);
            }

            return new byte[0];
        }
    }
}
