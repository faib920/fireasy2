// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using System;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 会话管理器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SessionManager<T>
    {
        private const string WS_SESSION_KEY = "ws_session";
        private const string WS_IDENTITY_KEY = "ws_identity";
        private TimeSpan _expire = TimeSpan.FromDays(5);

        /// <summary>
        /// 获取当前会话个数。
        /// </summary>
        public virtual int Count
        {
            get
            {
                var cacheMgr = CacheManagerFactory.CreateManager();
                var hashSet2 = cacheMgr.GetHashSet<int, string>(WS_IDENTITY_KEY);
                return (int)hashSet2.Count;
            }
        }

        /// <summary>
        /// 将客户端连接标识与用户标识进行关联。
        /// </summary>
        /// <param name="connectionId">客户端连接标识。></param>
        /// <param name="identity">用户标识。</param>
        public virtual void Add(string connectionId, T identity)
        {
            var cacheMgr = CacheManagerFactory.CreateManager();
            var hashSet1 = cacheMgr.GetHashSet<string, T>(WS_SESSION_KEY);
            var hashSet2 = cacheMgr.GetHashSet<string, string>(WS_IDENTITY_KEY);

            hashSet1.Add(connectionId, identity, new RelativeTime(_expire));
            hashSet2.Add(identity.ToString(), connectionId, new RelativeTime(_expire));
        }

        /// <summary>
        /// 移除指定的客户端连接标识。
        /// </summary>
        /// <param name="connectionId">客户端连接标识。></param>
        public virtual void Remove(string connectionId)
        {
            var cacheMgr = CacheManagerFactory.CreateManager();
            var hashSet1 = cacheMgr.GetHashSet<string, T>(WS_SESSION_KEY);
            var hashSet2 = cacheMgr.GetHashSet<string, string>(WS_IDENTITY_KEY);

            if (hashSet1.TryGet(connectionId, out T identity))
            {
                hashSet1.Remove(connectionId);
                hashSet2.Remove(identity.ToString());
            }
        }

        /// <summary>
        /// 通过用户标识查找户端连接标识。
        /// </summary>
        /// <param name="identity">用户标识。</param>
        /// <returns></returns>
        public virtual string FindConnection(T identity)
        {
            if (identity == null)
            {
                return string.Empty;
            }

            var cacheMgr = CacheManagerFactory.CreateManager();
            var hashSet = cacheMgr.GetHashSet<string, string>(WS_IDENTITY_KEY);
            if (hashSet.TryGet(identity.ToString(), out string key))
            {
                return key;
            }

            return string.Empty;
        }

        /// <summary>
        /// 通过客户端连接标识查找用户标识。
        /// </summary>
        /// <param name="connectionId">客户端连接标识。></param>
        /// <returns></returns>
        public virtual T FindIdentity(string connectionId)
        {
            var cacheMgr = CacheManagerFactory.CreateManager();
            var hashSet = cacheMgr.GetHashSet<string, T>(WS_SESSION_KEY);
            if (hashSet.TryGet(connectionId, out T value))
            {
                return value;
            }

            return default;
        }
    }
}
