// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Redis
{
    /// <summary>
    /// 基于 Redis 的缓存管理器。
    /// </summary>
    [ConfigurationSetting(typeof(RedisConfigurationSetting))]
    public class CacheManager : RedisComponent, IDistributedCacheManager
    {
        /// <summary>
        /// 将对象插入到缓存管理器中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="value">要插入到缓存的对象。</param>
        /// <param name="expire">对象存放于缓存中的有效时间，到期后将从缓存中移除。如果此值为 null，则默认有效时间为 30 分钟。</param>
        /// <param name="removeCallback">当对象从缓存中移除时，使用该回调方法通知应用程序。(在此类库中无效)</param>
        public T Add<T>(string cacheKey, T value, TimeSpan? expire = default(TimeSpan?), CacheItemRemovedCallback removeCallback = null)
        {
            var client = GetConnection();
            GetDb(client).StringSet(cacheKey, Serialize(value), expire);
            return value;
        }

        /// <summary>
        /// 将对象插入到缓存管理器中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="value">要插入到缓存的对象。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="removeCallback">当对象从缓存中移除时，使用该回调方法通知应用程序。</param>
        public T Add<T>(string cacheKey, T value, ICacheItemExpiration expiration, CacheItemRemovedCallback removeCallback = null)
        {
            var client = GetConnection();
            var expiry = GetExpiryTime(expiration);

            GetDb(client).StringSet(cacheKey, Serialize(value), expiry);
            return value;
        }

        /// <summary>
        /// 清除所有缓存。
        /// </summary>
        public void Clear()
        {
            var client = GetConnection();
            var db = GetDb(client);
            foreach (var endpoint in client.GetEndPoints())
            {
                var server = client.GetServer(endpoint);
                var keys = server.Keys();

                foreach (var key in keys)
                {
                    db.KeyDelete(key);
                }
            }
        }

        /// <summary>
        /// 确定缓存中是否包含指定的缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>如果缓存中包含指定缓存键的对象，则为 true，否则为 false。</returns>
        public bool Contains(string cacheKey)
        {
            var client = GetConnection();
            return GetDb(client).KeyExists(cacheKey);
        }

        /// <summary>
        /// 获取缓存中指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        public object Get(string cacheKey)
        {
            var client = GetConnection();
            var value = GetDb(client).StringGet(cacheKey);
            if (!string.IsNullOrEmpty(value))
            {
                return Deserialize<dynamic>(value);
            }

            return null;
        }

        /// <summary>
        /// 获取所有的 key。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetKeys()
        {
            var client = GetConnection();
            var server = client.GetServer(client.GetEndPoints()[0]);
            return server.Keys(Options.DefaultDatabase ?? 0).Select(s => s.ToString()).ToArray();
        }

        /// <summary>
        /// 从缓存中移除指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        public void Remove(string cacheKey)
        {
            var client = GetConnection();
            GetDb(client).KeyDelete(cacheKey);
        }

        /// <summary>
        /// 尝试获取指定缓存键的对象，如果没有则使用工厂函数添加对象到缓存中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public T TryGet<T>(string cacheKey, Func<T> factory, Func<ICacheItemExpiration> expiration = null)
        {
            var client = GetConnection();
            var db = GetDb(client);
            var token = Guid.NewGuid().ToString();
            var lockKey = $"{cacheKey}:LOCK_TOKEN";

            while (true)
            {
                if (db.LockTake(lockKey, token, TimeSpan.FromMinutes(30)))
                {
                    try
                    {
                        T result = default(T);
                        if (db.KeyExists(cacheKey))
                        {
                            var redisValue = db.StringGet(cacheKey);
                            if (!string.IsNullOrEmpty(redisValue))
                            {
                                CheckPredictToDelay(db, cacheKey, factory, expiration);

                                result = Deserialize<T>(redisValue);
                            }
                        }

                        return result != null? result : PutToCache(db, cacheKey, factory, expiration);
                    }
                    finally
                    {
                        db.LockRelease(lockKey, token);
                    }
                }
            }
        }

        /// <summary>
        /// 尝试获取指定缓存键的对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet<T>(string cacheKey, out T value)
        {
            var client = GetConnection();
            var db = GetDb(client);
            if (db.KeyExists(cacheKey))
            {
                var redisValue = db.StringGet(cacheKey);
                if (!string.IsNullOrEmpty(redisValue))
                {
                    value = Deserialize<T>(redisValue);
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        private IDatabase GetDb(IConnectionMultiplexer conn)
        {
            return conn.GetDatabase(Options.DefaultDatabase ?? 0);
        }

        /// <summary>
        /// 检查是否需要自动提前延期。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="cacheKey"></param>
        /// <param name="factory"></param>
        /// <param name="expiration"></param>
        private void CheckPredictToDelay<T>(IDatabase db, string cacheKey, Func<T> factory, Func<ICacheItemExpiration> expiration = null)
        {
            if (Setting.AdvanceDelay == null)
            {
                return;
            }

            var expiry = GetExpiryTime(expiration);

            if (expiry != null)
            {
                //判断过期时间，如果小于指定的时间比例，则提前进行预存
                var liveExpiry = db.KeyTimeToLive(cacheKey);
                if (liveExpiry != null &&
                    (liveExpiry.Value.TotalMilliseconds / expiry.Value.TotalMilliseconds) <= Setting.AdvanceDelay.Value)
                {
                    Task.Run(() =>
                        {
                            var value = factory();
                            db.StringSet(cacheKey, Serialize(value), expiry);
                        });
                }
            }
        }

        /// <summary>
        /// 将数据放入到缓存服务器中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="cacheKey"></param>
        /// <param name="factory"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        private T PutToCache<T>(IDatabase db, string cacheKey, Func<T> factory, Func<ICacheItemExpiration> expiration = null)
        {
            var expiry = GetExpiryTime(expiration);
            var value = factory();

            db.StringSet(cacheKey, Serialize(value), expiry);

            return value;
        }

        /// <summary>
        /// 获取缓存的有效期时间。
        /// </summary>
        /// <param name="expiration"></param>
        /// <returns></returns>
        private TimeSpan? GetExpiryTime(Func<ICacheItemExpiration> expiration)
        {
            return expiration == null ? null : GetExpiryTime(expiration());
        }

        /// <summary>
        /// 获取缓存的有效期时间。
        /// </summary>
        /// <param name="expiration"></param>
        /// <returns></returns>
        private TimeSpan? GetExpiryTime(ICacheItemExpiration expiration)
        {
            if (expiration == null)
            {
                return null;
            }

            if (expiration is RelativeTime relative) //相对时间
            {
                return relative.Expiration;
            }
            else if (expiration is AbsoluteTime absolute) //绝对时间
            {
                return absolute.ExpirationTime - DateTime.Now;
            }

            return null;
        }
    }
}
