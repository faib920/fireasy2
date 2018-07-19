// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 会话管理器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SessionManager<T> where T : IEquatable<T>
    {
        private ConcurrentDictionary<string, T> sessions = new ConcurrentDictionary<string, T>();

        /// <summary>
        /// 将客户端连接标识与用户标识进行关联。
        /// </summary>
        /// <param name="connectionId">客户端连接标识。></param>
        /// <param name="identity">用户标识。</param>
        /// <returns></returns>
        public bool Add(string connectionId, T identity)
        {
            KeyValuePair<string, T> kvp;
            if (!string.IsNullOrEmpty((kvp = sessions.FirstOrDefault(s => s.Value.Equals(identity))).Key))
            {
                sessions.TryRemove(kvp.Key, out T value);
            }

            return sessions.TryAdd(connectionId, identity);
        }

        /// <summary>
        /// 移除指定的客户端连接标识。
        /// </summary>
        /// <param name="connectionId">客户端连接标识。></param>
        /// <returns></returns>
        public bool Remove(string connectionId)
        {
            if (sessions.ContainsKey(connectionId))
            {
                return sessions.TryRemove(connectionId, out T value);
            }

            return false;
        }

        /// <summary>
        /// 通过用户标识查找户端连接标识。
        /// </summary>
        /// <param name="identity">用户标识。</param>
        /// <returns></returns>
        public string FindConnection(T identity)
        {
            return sessions.LastOrDefault(s => s.Value.Equals(identity)).Key;
        }

        /// <summary>
        /// 通过客户端连接标识查找用户标识。
        /// </summary>
        /// <param name="connectionId">客户端连接标识。></param>
        /// <returns></returns>
        public T FindIdentity(string connectionId)
        {
            if (sessions.TryGetValue(connectionId, out T value))
            {
                return value;
            }

            return default(T);
        }
    }
}
