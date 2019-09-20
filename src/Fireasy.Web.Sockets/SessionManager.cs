// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.Caching.Configuration;
using Fireasy.Common.Configuration;
using System;
using System.Linq;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 会话管理器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SessionManager<T> where T : IEquatable<T>
    {
        private const string prefix1 = "ws_session_";
        private const string prefix2 = "ws_identity_";

        /// <summary>
        /// 获取当前会话个数。
        /// </summary>
        public virtual int Count
        {
            get
            {
                var cacheMgr = CacheManagerFactory.CreateManager();
                return cacheMgr.GetKeys(prefix2 + "*").Count();
            }
        }

        /// <summary>
        /// 将客户端连接标识与用户标识进行关联。
        /// </summary>
        /// <param name="connectionId">客户端连接标识。></param>
        /// <param name="identity">用户标识。</param>
        public virtual void Add(string connectionId, T identity)
        {
            var section = ConfigurationUnity.GetSection<CachingConfigurationSection>();

            var cacheMgr = CacheManagerFactory.CreateManager();
            cacheMgr.Add(prefix1 + connectionId, identity, new RelativeTime(TimeSpan.FromDays(5)));
            cacheMgr.Add(prefix2 + identity, connectionId, new RelativeTime(TimeSpan.FromDays(5)));
        }

        /// <summary>
        /// 移除指定的客户端连接标识。
        /// </summary>
        /// <param name="connectionId">客户端连接标识。></param>
        public virtual void Remove(string connectionId)
        {
            var cacheMgr = CacheManagerFactory.CreateManager();
            var identity = cacheMgr.Get<T>(prefix1 + connectionId);
            if (identity != null)
            {
                cacheMgr.Remove(prefix1 + connectionId);
                cacheMgr.Remove(prefix2 + identity);
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
            if (cacheMgr.TryGet(prefix2 + identity, out string key))
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
            if (cacheMgr.TryGet(prefix1 + connectionId, out T value))
            {
                return value;
            }

            return default;
        }
    }
}
