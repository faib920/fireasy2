// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// WebSocket 处理的抽象类。
    /// </summary>
    public abstract class WebSocketHandler : DisposableBase, IClientProxy
    {
        private DateTime _lastReceivedTime = DateTime.Now;
        private Timer _timer;
        private bool _isClosing = false;
        private CancellationTokenSource _cancelToken;

        /// <summary>
        /// 获取连接唯一标识符。
        /// </summary>
        public string ConnectionId { get; private set; }

        /// <summary>
        /// 获取当前的 <see cref="WebSocketAcceptContext"/> 对象。
        /// </summary>
        public WebSocketAcceptContext AcceptContext { get; private set; }

        /// <summary>
        /// 获取客户端集合。
        /// </summary>
        public IClientManager Clients { get; private set; }

        /// <summary>
        /// 初始化 <see cref="WebSocketHandler"/> 类的新实例。
        /// </summary>
        public WebSocketHandler()
        {
            ConnectionId = Guid.NewGuid().ToString();
        }

        internal static async Task Accept<T>(WebSocketAcceptContext acceptContext) where T : WebSocketHandler, new()
        {
            await Accept(typeof(T), acceptContext);
        }

        internal static async Task Accept(Type handlerType, WebSocketAcceptContext acceptContext)
        {
            using (var handler = handlerType.New<WebSocketHandler>())
            {
                await Accept(handler, acceptContext);
            }
        }

        internal static async Task Accept(WebSocketHandler handler, WebSocketAcceptContext acceptContext)
        {
            handler._cancelToken = new CancellationTokenSource();
            handler.AcceptContext = acceptContext;

            handler.Clients = new WrapClientManager(ClientManagerCache.GetManager(handler.GetType(), acceptContext), handler);

            try
            {
                await handler.InvokeAsync();
            }
            catch (Exception exp)
            {
                handler.OnFatalError(exp);
            }
        }

        DateTime IClientProxy.AliveTime
        {
            get { return _lastReceivedTime; }
        }

        async Task IClientProxy.SendAsync(string method, params object[] arguments)
        {
            var message = new InvokeMessage(method, 0, arguments);
            var json = AcceptContext.Option.Formatter.FormatMessage(message);
            var bytes = AcceptContext.Option.Encoding.GetBytes(json);

            try
            {
                if (IsValidState(WebSocketState.Open, WebSocketState.CloseReceived))
                {
                    await AcceptContext.WebSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, _cancelToken.Token);
                }
            }
            catch (Exception exp)
            {
                OnInvokeError(message, new WebSocketHandleException(ConnectionId, "调用客户端方法时发生异常。", exp));
            }
        }

        private async Task InvokeAsync()
        {
            ListenHeartBeat();

            OnConnected();
            Clients.Add(ConnectionId, this);

            var buffer = new byte[AcceptContext.Option.ReceiveBufferSize];
            var data = new DataBuffer();

            while (AcceptContext.WebSocket.State == WebSocketState.Open)
            {
                var result = await AcceptContext.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancelToken.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await AcceptContext.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close response received", _cancelToken.Token);
                    break;
                }

                data.AddBuffer(buffer, result.Count);

                if (result.EndOfMessage)
                {
                    _lastReceivedTime = DateTime.Now;

                    var bytes = HandleResult(result.MessageType, data);

                    data.Clear();

                    if (bytes != null)
                    {
                        await AcceptContext.WebSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), result.MessageType, result.EndOfMessage, _cancelToken.Token);
                    }
                }

                Thread.Sleep(100);
            }

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
        /// 检测到心跳。
        /// </summary>
        protected virtual void OnHeartBeating()
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
        /// 发生致命异常时的通知。
        /// </summary>
        /// <param name="exception">异常。</param>
        protected virtual void OnFatalError(Exception exception)
        {
        }

        /// <summary>
        /// 用户认证。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual bool OnAuthorizing(AuthorizeContext context)
        {
            return true;
        }

        protected override bool Dispose(bool disposing)
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }

            if (_cancelToken != null)
            {
                _cancelToken.Dispose();
            }

            if (!_isClosing)
            {
                _isClosing = true;
                Clients.Remove(ConnectionId);
                OnDisconnected();

                if (AcceptContext != null && AcceptContext.WebSocket != null && AcceptContext.WebSocket.CloseStatus == null)
                {
                    AcceptContext.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None)
                        .ContinueWith(t => AcceptContext.WebSocket.Dispose());
                }
            }

            return base.Dispose(disposing);
        }

        /// <summary>
        /// 处理数据结果。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] HandleResult(WebSocketMessageType type, byte[] data)
        {
            if (data.Length == 1 && data[0] == '\0')
            {
                Clients.Refresh(ConnectionId);
                OnHeartBeating();
                return null;
            }

            if (type == WebSocketMessageType.Binary)
            {
                OnReceived(data);
            }
            else if (type == WebSocketMessageType.Text)
            {
                var content = AcceptContext.Option.Encoding.GetString(data);
                InvokeMessage message;
                try
                {
                    OnReceived(content);
                    message = AcceptContext.Option.Formatter.ResolveMessage(content);
                }
                catch (Exception exp)
                {
                    OnResolveError(content, exp);
                    return null;
                }

                Type returnType = null;

                try
                {
                    var method = FindMethod(message);

                    var authContext = new AuthorizeContext(AcceptContext.User, method, message.Arguments);
                    if (!OnAuthorizing(authContext))
                    {
                        throw new Exception($"未通过认证 {message.Method}。");
                    }

                    var arguments = ResolveArguments(method, message);

                    var result = method.FastInvoke(this, arguments);
                    if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        returnType = method.ReturnType.GetGenericArguments()[0];
                        result = method.ReturnType.GetProperty("Result").GetValue(result);
                    }
                    else if (method.ReturnType != typeof(void) && method.ReturnType != typeof(Task))
                    {
                        returnType = method.ReturnType;
                    }

                    if (returnType != null)
                    {
                        message.IsReturn = 1;
                        return ReturnValue(message, result);
                    }
                }
                catch (Exception exp)
                {
                    OnInvokeError(message, new WebSocketHandleException(ConnectionId, "处理接收到的数据时发生异常。", exp));
                    return ReturnValue(message, returnType?.GetDefaultValue());
                }
            }

            return null;
        }

        /// <summary>
        /// 返回数据。
        /// </summary>
        /// <param name="message"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private byte[] ReturnValue(InvokeMessage message, object result)
        {
            if (message.IsReturn == 0 || result == null)
            {
                return new byte[0];
            }

            var retMsg = new InvokeMessage(message.Method, 1, new[] { result })
            {
                IsReturn = message.IsReturn
            };

            return AcceptContext.Option.Encoding.GetBytes(AcceptContext.Option.Formatter.FormatMessage(retMsg));
        }

        /// <summary>
        /// 查找调用的方法。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private MethodInfo FindMethod(InvokeMessage message)
        {
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(s => s.Name.Equals(message.Method, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            if (methods.Length == 0)
            {
                throw new Exception($"没有找到方法 {message.Method}。");
            }

            if (methods.Length > 1)
            {
                throw new AmbiguousMatchException($"无法从多个重载方法匹配 {message.Method}，只允许定义一个方法。");
            }

            return methods[0];
        }

        /// <summary>
        /// 解析方法的参数。
        /// </summary>
        /// <param name="method"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private object[] ResolveArguments(MethodInfo method, InvokeMessage message)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != message.Arguments.Length)
            {
                throw new Exception($"方法 {message.Method} 参数不匹配。");
            }

            //处理参数
            var arguments = new object[message.Arguments.Length];
            for (var i = 0; i < arguments.Length; i++)
            {
                if (message.Arguments[i] != null && parameters[i].ParameterType != message.Arguments[i].GetType())
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
        /// 手动关闭。
        /// </summary>
        private void ManualClose()
        {
            if (_isClosing)
            {
                return;
            }

            Dispose(true);
        }

        /// <summary>
        /// 监听心跳包。
        /// </summary>
        private void ListenHeartBeat()
        {
            if (AcceptContext.Option.HeartbeatInterval == TimeSpan.MaxValue)
            {
                return;
            }

            _timer = new Timer(o =>
                {
                    //3次容错
                    if ((DateTime.Now - _lastReceivedTime).TotalMilliseconds >=
                        AcceptContext.Option.HeartbeatInterval.TotalMilliseconds * AcceptContext.Option.HeartbeatTryTimes)
                    {
                        _cancelToken.Cancel(false);
                        ManualClose();
                    }
                }, null, AcceptContext.Option.HeartbeatInterval, AcceptContext.Option.HeartbeatInterval);
        }

        private class DataBuffer : List<byte>
        {
            public static implicit operator byte[](DataBuffer buffer)
            {
                return buffer.ToArray();
            }

            public void AddBuffer(byte[] buffer, int length)
            {
                for (var i = 0; i < length; i++)
                {
                    Add(buffer[i]);
                }
            }
        }

        private bool IsValidState(params WebSocketState[] status)
        {
            return Array.IndexOf(status, AcceptContext.WebSocket.State) >= 0;
        }
    }
}
