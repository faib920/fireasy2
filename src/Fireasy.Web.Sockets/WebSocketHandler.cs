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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    public abstract class WebSocketHandler : IClientProxy, IDisposable
    {
        private WebSocketAcceptContext acceptContext;
        private DateTime lastHeartbeatTime = DateTime.Now;
        private Timer timer;
        private bool isDisposed = false;
        private bool isClosing = false;

        /// <summary>
        /// 获取连接唯一标识符。
        /// </summary>
        public string ConnectionId { get; private set; }

        /// <summary>
        /// 获取客户端集合。
        /// </summary>
        public ClientManager Clients { get; private set; }

        /// <summary>
        /// 获取组集合。
        /// </summary>
        public GroupManager Groups { get; private set; }

        public WebSocketHandler()
        {
            this.ConnectionId = Guid.NewGuid().ToString();
        }

        ~WebSocketHandler()
        {
            Dispose(false);
        }

        public static async Task Accept<T>(WebSocketAcceptContext acceptContext) where T : WebSocketHandler, new()
        {
            await Accept(typeof(T), acceptContext);
        }

        public static async Task Accept(Type handlerType, WebSocketAcceptContext acceptContext)
        {
            await Accept(handlerType.New<WebSocketHandler>(), acceptContext);
        }

        public static async Task Accept(WebSocketHandler handler, WebSocketAcceptContext acceptContext)
        {
            handler.acceptContext = acceptContext;
            handler.Clients = ClientManager.GetManager(handler.GetType());
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

            await acceptContext.WebSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task Invoke()
        {
            ListenHeartBeat();

            OnConnected();
            Clients.Add(ConnectionId, this);

            var buffer = new byte[1024 * 4];
            var result = await acceptContext.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                var bytes = HandleResult(result.MessageType, buffer, result.Count);

                if (bytes.Length > 0)
                {
                    await acceptContext.WebSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                }

                result = await acceptContext.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await acceptContext.WebSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

            ManualClose();
        }

        /// <summary>
        /// 连接上的通知。
        /// </summary>
        protected virtual void OnConnected()
        {
        }

        /// <summary>
        /// 断开连接时的通知。
        /// </summary>
        protected virtual void OnDisconnected()
        {
        }

        /// <summary>
        /// 接收到文本数据时的通知。
        /// </summary>
        /// <param name="content"></param>
        protected virtual void OnReceived(string content)
        {
        }

        /// <summary>
        /// 接收到二进制数据时的通知。
        /// </summary>
        /// <param name="bytes"></param>
        protected virtual void OnReceived(byte[] bytes)
        {
        }

        /// <summary>
        /// 数据解析失败时的通知。
        /// </summary>
        /// <param name="content">传递的数据内容。</param>
        /// <param name="exception">异常。</param>
        protected virtual void OnResolveError(string content, Exception exception)
        {
        }

        /// <summary>
        /// 调用方法失败时的通知。
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception">异常。</param>
        protected virtual void OnInvokeError(InvokeMessage message, Exception exception)
        {
        }

        /// <summary>
        /// 释放对象所占用的所有资源。
        /// </summary>
            public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (acceptContext != null && acceptContext.WebSocket != null)
            {
                acceptContext.WebSocket.Dispose();
            }

            if (timer != null)
            {
                timer.Dispose();
            }

            isDisposed = true;
        }

        /// <summary>
        /// 处理数据结果。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private byte[] HandleResult(WebSocketMessageType type, byte[] buffer, int length)
        {
            ProcessHeartBeat();

            if (type == WebSocketMessageType.Binary)
            {
                OnReceived(buffer);
            }
            else if (type == WebSocketMessageType.Text)
            {
                var content = acceptContext.Option.Encoding.GetString(buffer, 0, length);
                InvokeMessage message = null;

                try
                {
                    OnReceived(content);
                    message = acceptContext.Option.Formatter.ResolveMessage(content);
                }
                catch (Exception exp)
                {
                    OnResolveError(content, exp);
                    return new byte[0];
                }

                try
                {
                    var method = FindMethod(message);
                    if (method == null)
                    {
                        throw new Exception($"没有发现方法 {message.Method}");
                    }

                    var arguments = ResolveArguments(method, message);

                    var result = method.FastInvoke(this, arguments);
                    if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        result = method.ReturnType.GetProperty("Result").GetValue(result);
                    }

                    if (method.ReturnType != typeof(void))
                    {
                        var ret = new InvokeMessage(message.Method, 1, new[] { result });
                        return acceptContext.Option.Encoding.GetBytes(acceptContext.Option.Formatter.FormatMessage(ret));
                    }
                }
                catch (Exception exp)
                {
                    OnInvokeError(message, exp);
                }
            }

            return new byte[0];
        }

        private MethodInfo FindMethod(InvokeMessage message)
        {
            return this.GetType().GetMethod(message.Method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        }

        private object[] ResolveArguments(MethodInfo method, InvokeMessage message)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != message.Arguments.Length)
            {
                throw new Exception($"方法 {message.Method} 参数不匹配");
            }

            //处理参数
            var arguments = new object[message.Arguments.Length];
            for (var i = 0; i < arguments.Length; i++)
            {
                if (parameters[i].ParameterType != message.Arguments[i].GetType())
                {
                    arguments[i] = message.Arguments[i].ToType(parameters[i].ParameterType);
                }
                else
                {
                    arguments[i] = message.Arguments[i];
                }
            }

            return arguments;
        }

        /// <summary>
        /// 处理心跳包。
        /// </summary>
        private void ProcessHeartBeat()
        {
            lastHeartbeatTime = DateTime.Now;
        }

        /// <summary>
        /// 手动关闭。
        /// </summary>
        private void ManualClose()
        {
            if (isClosing)
            {
                return;
            }

            isClosing = true;
            OnDisconnected();
            Clients.Remove(ConnectionId);
            Dispose(true);
        }

        /// <summary>
        /// 监听心跳包。
        /// </summary>
        private void ListenHeartBeat()
        {
            timer = new Timer(o =>
                {
                    //3次容错
                    if ((DateTime.Now - lastHeartbeatTime).TotalMilliseconds >=
                        acceptContext.Option.HeartbeatInterval.TotalMilliseconds * acceptContext.Option.HeartbeatTryTimes)
                    {
                        if (acceptContext.WebSocket.State == WebSocketState.Open)
                        {
                            acceptContext.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        }

                        ManualClose();
                    }
                }, null, acceptContext.Option.HeartbeatInterval, acceptContext.Option.HeartbeatInterval);
        }
    }
}
