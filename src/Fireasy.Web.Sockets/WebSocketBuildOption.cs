// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// WebSocket 参数。
    /// </summary>
    public class WebSocketBuildOption
    {
        private Dictionary<string, Type> mapping = new Dictionary<string, Type>();

#if !NETSTANDARD
        public static readonly WebSocketBuildOption Default = new WebSocketBuildOption();
#endif
        /// <summary>
        /// 获取或设置服务标识。
        /// </summary>
        public string AliveKey { get; set; } = string.Concat("fws_", Guid.NewGuid().ToString("N"));

        /// <summary>
        /// 获取或设置保持活动状态的时间间隔。
        /// </summary>
        public TimeSpan KeepAliveInterval { get; set; }

        /// <summary>
        /// 获取或设置接收数据的缓冲区大小。默认为 1024 字节。
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 1024;

        /// <summary>
        /// 获取或设置心跳的时间间隔。默认 30 秒。
        /// </summary>
        public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 获取或设置心跳容错的次数。默认为 3 次。
        /// </summary>
        public int HeartbeatTryTimes { get; set; } = 3;

        /// <summary>
        /// 获取或设置是否分布式部署。
        /// </summary>
        public bool Distributed { get; set; }

        /// <summary>
        /// 获取或设置字符编码。
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 获取或设置消息格式化器。
        /// </summary>
        public IMessageFormatter Formatter { get; set; } = new MessageFormatter();

        /// <summary>
        /// 将处理类映射到指定的路径。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        public void MapHandler<T>(string path) where T : WebSocketHandler
        {
            mapping.Add(path, typeof(T));
        }

        /// <summary>
        /// 获取与路径相匹配的处理类。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Type GetHandlerType(string path)
        {
            if (mapping.TryGetValue(path, out Type type))
            {
                return type;
            }

            return null;
        }
    }
}
