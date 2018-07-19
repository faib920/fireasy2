// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// WebSocket 参数。
    /// </summary>
    public class WebSocketBuildOption
    {
        private Dictionary<string, Type> mapping = new Dictionary<string, Type>();

        /// <summary>
        /// 获取或设置保持活动状态的时间间隔。
        /// </summary>
        public TimeSpan KeepAliveInterval { get; set; }

        /// <summary>
        /// 获取或设置接收数据的缓冲区大小。
        /// </summary>
        public int ReceiveBufferSize { get; set; }

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
