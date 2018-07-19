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
            await client.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// 发送数据并接收返回数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task<byte[]> SendAndReturnAsync(byte[] bytes)
        {
            await client.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            var buffer = new byte[1024 * 4];
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
            var serializer = new JsonSerializer();

            var message = new InvokeMessage(method, 0, arguments);
            var json = serializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);

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
            var serializer = new JsonSerializer();

            var message = new InvokeMessage(method, 0, arguments);
            var json = serializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            await client.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);

            var buffer = new byte[1024 * 4];
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.CloseStatus.HasValue)
            {
                return default(T);
            }

            var obj = serializer.Deserialize<InvokeMessage>(Encoding.UTF8.GetString(buffer, 0, result.Count));
            if (obj.Direction == 1)
            {
                return obj.Arguments[0].To<T>();
            }

            return default(T);
        }
    }
}
