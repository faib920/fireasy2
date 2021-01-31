// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using System;
using System.Collections.Generic;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 会话管理器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SessionManager<T>
    {
        private readonly string _sessionCacheKey;
        private readonly string _identityCacheKey;
        private TimeSpan _expire;

        /// <summary>
        /// 初始化 <see cref="SessionManager{T}"/> 的新实例。
        /// </summary>
        public SessionManager()
            : this (string.Empty, TimeSpan.FromMinutes(5))
        {
        }

        /// <summary>
        /// 初始化 <see cref="SessionManager{T}"/> 的新实例。
        /// </summary>
        /// <param name="appKey">应用标识。</param>
        /// <param name="expire">会话的过期时间。</param>
        public SessionManager(string appKey, TimeSpan expire)
        {
            _expire = expire;
            _sessionCacheKey = $"{appKey}:session_{typeof(T).Name}";
            _identityCacheKey = $"{appKey}:identity_{typeof(T).Name}";
        }

        /// <summary>
        /// 初始化 <see cref="SessionManager{T}"/> 的新实例。
        /// </summary>
        /// <param name="option">参数。</param>
        public SessionManager(WebSocketBuildOption option)
        {
            _expire = TimeSpan.FromMilliseconds(option.HeartbeatInterval.TotalMilliseconds * 5);
            _sessionCacheKey = $"{option.AppKey}:session_{typeof(T).Name}";
            _identityCacheKey = $"{option.AppKey}:identity_{typeof(T).Name}";
        }

        /// <summary>
        /// 获取当前会话个数。
        /// </summary>
        public virtual int Count
        {
            get
            {
                var cacheMgr = CacheManagerFactory.CreateManager();
                var hashSet2 = cacheMgr.GetHashSet<string, string>(_identityCacheKey);
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
            var hashSet1 = cacheMgr.GetHashSet<string, T>(_sessionCacheKey);
            var hashSet2 = cacheMgr.GetHashSet<string, string>(_identityCacheKey);

            hashSet1.Add(connectionId, identity, new RelativeTime(_expire));
            hashSet2.Add(identity.ToString(), connectionId, new RelativeTime(_expire));
        }

        /// <summary>
        /// 刷新会话时间。
        /// </summary>
        /// <param name="connectionId">客户端连接标识。></param>
        public virtual void Refresh(string connectionId)
        {
            var cacheMgr = CacheManagerFactory.CreateManager();
            var hashSet1 = cacheMgr.GetHashSet<string, T>(_sessionCacheKey);
            var hashSet2 = cacheMgr.GetHashSet<string, string>(_identityCacheKey);

            if (hashSet1.TryGet(connectionId, out T identity))
            {
                hashSet1.Add(connectionId, identity, new RelativeTime(_expire));
                hashSet2.Add(identity.ToString(), connectionId, new RelativeTime(_expire));
            }
        }

        /// <summary>
        /// 移除指定的客户端连接标识。
        /// </summary>
        /// <param name="connectionId">客户端连接标识。></param>
        public virtual void Remove(string connectionId)
        {
            var cacheMgr = CacheManagerFactory.CreateManager();
            var hashSet1 = cacheMgr.GetHashSet<string, T>(_sessionCacheKey);
            var hashSet2 = cacheMgr.GetHashSet<string, string>(_identityCacheKey);

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
            var hashSet = cacheMgr.GetHashSet<string, string>(_identityCacheKey);
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
            var hashSet = cacheMgr.GetHashSet<string, T>(_sessionCacheKey);
            if (hashSet.TryGet(connectionId, out T value))
            {
                return value;
            }

            return default;
        }

        /// <summary>
        /// 获取客户端连接与用户标识的映射表。
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, T> GetIdentityTable()
        {
            var dict = new Dictionary<string, T>();
            var cacheMgr = CacheManagerFactory.CreateManager();
            var hashSet = cacheMgr.GetHashSet<string, T>(_sessionCacheKey);
            foreach (var key in hashSet.GetKeys())
            {
                if (hashSet.TryGet(key, out T value))
                {
                    dict.Add(key, value);
                }
            }

            return dict;
        }

        /// <summary>
        /// 获取用户标识列表。
        /// </summary>
        /// <returns></returns>
        public virtual List<T> GetIdentityList()
        {
            var list = new List<T>();
            var cacheMgr = CacheManagerFactory.CreateManager();
            var hashSet1 = cacheMgr.GetHashSet<string, T>(_sessionCacheKey);
            var hashSet2 = cacheMgr.GetHashSet<string, string>(_identityCacheKey);

            foreach (var key in hashSet2.GetKeys())
            {
                if (hashSet2.TryGet(key, out string cid) && hashSet1.TryGet(cid, out T value))
                {
                    list.Add(value);
                }
            }

            return list;
        }
    }
}
