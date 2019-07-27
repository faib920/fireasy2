// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// WebSocket 客户端。
    /// </summary>
    public class WebSocketClient
    {
        private ClientWebSocket client;

        /// <summary>
        /// 获取或设置接收数据的缓冲区大小。
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 1024 * 4;

        /// <summary>
        /// 获取或设置编码格式。
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 获取或设置消息格式化器。
        /// </summary>
        public IMessageFormatter Formatter { get; set; } = new MessageFormatter();

        /// <summary>
        /// 打开连接。
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task StartAsync(string uri)
        {
            client = new ClientWebSocket();
            await client.ConnectAsync(new System.Uri(uri), CancellationToken.None);
        }

        /// <summary>
        /// 关闭连接。
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            if (client != null)
            {
                await client.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);
            }
        }

        /// <summary>
        /// 发送数据。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task SendAsync(byte[] bytes)
        {
            await client.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        /// <summary>
        /// 发送数据并接收返回数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task<byte[]> SendBinaryAsync(byte[] bytes)
        {
            await client.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
            var buffer = new byte[ReceiveBufferSize];
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (!result.CloseStatus.HasValue)
            {
                bytes = new byte[result.Count];
                Array.Copy(buffer, bytes, bytes.Length);
                return buffer;
            }

            return new byte[0];
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="method">方法名。</param>
        /// <param name="arguments">调用的参数。</param>
        /// <returns></returns>
        public async Task SendAsync(string method, params object[] arguments)
        {
            var message = new InvokeMessage(method, 0, arguments);
            var json = Formatter.FormatMessage(message);
            var bytes = Encoding.GetBytes(json);

            await client.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method">方法名。</param>
        /// <param name="arguments">调用的参数。</param>
        /// <returns></returns>
        public async Task<T> SendAsync<T>(string method, params object[] arguments)
        {
            var message = new InvokeMessage(method, 0, arguments) { IsReturn = 1 };
            var json = Formatter.FormatMessage(message);
            var bytes = Encoding.GetBytes(json);

            await client.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);

            var buffer = new byte[ReceiveBufferSize];
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.CloseStatus.HasValue)
            {
                return default(T);
            }

            try
            {
                var obj = Formatter.ResolveMessage(Encoding.GetString(buffer, 0, result.Count));
                if (obj != null && obj.Direction == 1)
                {
                    return obj.Arguments[0].To<T>();
                }
            }
            catch (Exception exp)
            {
                throw new InvalidOperationException($"发送 {method} 时发生异常。", exp);
            }

            return default(T);
        }
    }
}
