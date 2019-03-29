// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.Configuration;
#if NETSTANDARD
using CSRedis;
#else
using StackExchange.Redis;
using System.Linq;
#endif
using System;
using System.Collections.Generic;
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
#if NETSTANDARD
            client.Set(cacheKey, Serialize(value), expire == null ? -1 : (int)expire.Value.TotalSeconds);
#else
            GetDb(client).StringSet(cacheKey, Serialize(value), expire);
#endif
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
            var expiry = GetExpirationTime(expiration);

#if NETSTANDARD
            client.Set(cacheKey, Serialize(value), expiry == null ? -1 : (int)expiry.Value.TotalSeconds);
#else
            GetDb(client).StringSet(cacheKey, Serialize(value), expiry);
#endif
            return value;
        }

        /// <summary>
        /// 清除所有缓存。
        /// </summary>
        public void Clear()
        {
            var client = GetConnection();
#if NETSTANDARD
            client.Del(client.Keys("*"));
#else
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
#endif
        }

        /// <summary>
        /// 确定缓存中是否包含指定的缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>如果缓存中包含指定缓存键的对象，则为 true，否则为 false。</returns>
        public bool Contains(string cacheKey)
        {
            var client = GetConnection();
#if NETSTANDARD
            return client.Exists(cacheKey);
#else
            return GetDb(client).KeyExists(cacheKey);
#endif
        }

        /// <summary>
        /// 获取缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns></returns>
        public TimeSpan? GetExpirationTime(string cacheKey)
        {
            var client = GetConnection();
#if NETSTANDARD
            var time = client.ObjectIdleTime(cacheKey);
            return time == null ? (TimeSpan?)null : TimeSpan.FromSeconds((double)time);
#else
            return GetDb(client).KeyTimeToLive(cacheKey);
#endif
        }

        /// <summary>
        /// 获取缓存中指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        public object Get(string cacheKey)
        {
            var client = GetConnection();
#if NETSTANDARD
            var value = client.Get(cacheKey);
#else
            var value = GetDb(client).StringGet(cacheKey);
#endif
            if (!string.IsNullOrEmpty(value))
            {
                return Deserialize<dynamic>(value);
            }

            return null;
        }

        /// <summary>
        /// 获取缓存中指定缓存键的对象。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        public T Get<T>(string cacheKey)
        {
            var client = GetConnection();
#if NETSTANDARD
            var value = client.Get(cacheKey);
#else
            var value = GetDb(client).StringGet(cacheKey);
#endif
            if (!string.IsNullOrEmpty(value))
            {
                return (T)Deserialize(typeof(T), value);
            }

            return default(T);
        }

        /// <summary>
        /// 获取所有的 key。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetKeys()
        {
            var client = GetConnection();
#if NETSTANDARD
            return client.Keys("*");
#else
            var server = client.GetServer(client.GetEndPoints()[0]);
            return server.Keys(Options.DefaultDatabase ?? 0).Select(s => s.ToString()).ToArray();
#endif
        }

        /// <summary>
        /// 从缓存中移除指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        public void Remove(string cacheKey)
        {
            var client = GetConnection();
#if NETSTANDARD
            client.Del(cacheKey);
#else
            GetDb(client).KeyDelete(cacheKey);
#endif
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
#if NETSTANDARD
            return LockDb(client, cacheKey, () =>
                {
                    if (client.Exists(cacheKey))
                    {
                        var redisValue = client.Get(cacheKey);
                        if (!string.IsNullOrEmpty(redisValue))
                        {
                            CheckPredictToDelay(client, cacheKey, factory, expiration);

                            return Deserialize<T>(redisValue);
                        }
                    }

                    return PutToCache(client, cacheKey, factory, expiration);
                });
#else
            var db = GetDb(client);

            return LockDb(db, cacheKey, () =>
                {
                    if (db.KeyExists(cacheKey))
                    {
                        var redisValue = db.StringGet(cacheKey);
                        if (!string.IsNullOrEmpty(redisValue))
                        {
                            CheckPredictToDelay(db, cacheKey, factory, expiration);

                            return Deserialize<T>(redisValue);
                        }
                    }

                    return PutToCache(db, cacheKey, factory, expiration);
                });
#endif
        }

        /// <summary>
        /// 尝试获取指定缓存键的对象，如果没有则使用工厂函数添加对象到缓存中。
        /// </summary>
        /// <param name="dataType">数据类型。</param>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public object TryGet(Type dataType, string cacheKey, Func<object> factory, Func<ICacheItemExpiration> expiration = null)
        {
            var client = GetConnection();
#if NETSTANDARD
            return LockDb(client, cacheKey, () =>
                {
                    if (client.Exists(cacheKey))
                    {
                        var redisValue = client.Get(cacheKey);
                        if (!string.IsNullOrEmpty(redisValue))
                        {
                            CheckPredictToDelay(client, cacheKey, factory, expiration);

                            return Deserialize(dataType, redisValue);
                        }
                    }

                    return PutToCache(client, cacheKey, factory, expiration);
                });
#else
            var db = GetDb(client);

            return LockDb(db, cacheKey, () =>
                {
                    if (db.KeyExists(cacheKey))
                    {
                        var redisValue = db.StringGet(cacheKey);
                        if (!string.IsNullOrEmpty(redisValue))
                        {
                            CheckPredictToDelay(db, cacheKey, factory, expiration);

                            return Deserialize(dataType, redisValue);
                        }
                    }

                    return PutToCache(db, cacheKey, factory, expiration);
                });
#endif
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
#if NETSTANDARD
            if (client.Exists(cacheKey))
            {
                var redisValue = client.Get(cacheKey);
                if (!string.IsNullOrEmpty(redisValue))
                {
                    value = Deserialize<T>(redisValue);
                    return true;
                }
            }
#else
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
#endif

            value = default(T);
            return false;
        }

        /// <summary>
        /// 尝试获取增量。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于初始化数据的工厂函数。</param>
        /// <param name="step">递增的步数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public long TryIncrement(string cacheKey, Func<long> factory, int step = 1, Func<ICacheItemExpiration> expiration = null)
        {
            var client = GetConnection();
#if NETSTANDARD
            return LockDb(client, cacheKey, () =>
                {
                    long ret = 0;
                    if (client.Exists(cacheKey))
                    {
                        ret = client.IncrBy(cacheKey, step);
                    }
                    else
                    {
                        ret = client.IncrBy(cacheKey, factory() + step);

                        SetKeyExpiration(client, cacheKey, expiration);
                    }

                    return ret;
                });
#else
            var db = GetDb(client);

            return LockDb(db, cacheKey, () =>
                {
                    long ret = 0;
                    if (db.KeyExists(cacheKey))
                    {
                        ret = db.StringIncrement(cacheKey, step);
                    }
                    else
                    {
                        ret = db.StringIncrement(cacheKey, factory() + step);

                        SetKeyExpiration(db, cacheKey, expiration);
                    }

                    return ret;
                });
#endif
        }

        /// <summary>
        /// 尝试获取减量。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于初始化数据的工厂函数。</param>
        /// <param name="step">递减的步数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public long TryDecrement(string cacheKey, Func<long> factory, int step = 1, Func<ICacheItemExpiration> expiration = null)
        {
            var client = GetConnection();
#if NETSTANDARD
            return LockDb(client, cacheKey, () =>
                {
                    long ret = 0;
                    if (client.Exists(cacheKey))
                    {
                        ret = client.IncrBy(cacheKey, -step);
                    }
                    else
                    {
                        client.IncrBy(cacheKey, factory());
                        ret = client.IncrBy(cacheKey, -step);

                        SetKeyExpiration(client, cacheKey, expiration);
                    }

                    return ret;
                });
#else
            var db = GetDb(client);

            return LockDb(db, cacheKey, () =>
                {
                    long ret = 0;
                    if (db.KeyExists(cacheKey))
                    {
                        ret = db.StringDecrement(cacheKey, step);
                    }
                    else
                    {
                        db.StringIncrement(cacheKey, factory());
                        ret = db.StringDecrement(cacheKey, step);

                        SetKeyExpiration(db, cacheKey, expiration);
                    }

                    return ret;
                });
#endif
        }

#if !NETSTANDARD
        private IDatabase GetDb(IConnectionMultiplexer conn)
        {
            return conn.GetDatabase(Options.DefaultDatabase ?? 0);
        }

        /// <summary>
        /// 锁定DB进行数据操作。
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="db"></param>
        /// <param name="cacheKey"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        private TOut LockDb<TOut>(IDatabase db, string cacheKey, Func<TOut> func)
        {
            var token = Guid.NewGuid().ToString();
            var lockKey = $"{cacheKey}:LOCK_TOKEN";

            while (true)
            {
                if (db.LockTake(lockKey, token, TimeSpan.FromSeconds(10)))
                {
                    try
                    {
                        return func();
                    }
                    finally
                    {
                        db.LockRelease(lockKey, token);
                    }
                }
            }
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

            var expiry = GetExpirationTime(expiration);

            if (expiry != null)
            {
                //判断过期时间，如果小于指定的时间比例，则提前进行预存
                var liveTime = db.KeyTimeToLive(cacheKey);
                if (liveTime != null &&
                    (liveTime.Value.TotalMilliseconds / expiry.Value.TotalMilliseconds) <= Setting.AdvanceDelay.Value)
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
            var expiry = GetExpirationTime(expiration);
            var value = factory();

            db.StringSet(cacheKey, Serialize(value), expiry);

            return value;
        }

        private void SetKeyExpiration(IDatabase db, string cacheKey, Func<ICacheItemExpiration> expiration)
        {
            var expiry = GetExpirationTime(expiration);
            if (expiry != null)
            {
                db.KeyExpire(cacheKey, expiry);
            }
        }
#else
        /// <summary>
        /// 锁定DB进行数据操作。
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="client"></param>
        /// <param name="cacheKey"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        private TOut LockDb<TOut>(CSRedisClient client, string cacheKey, Func<TOut> func)
        {
            var token = Guid.NewGuid().ToString();
            var lockKey = $"{cacheKey}:LOCK_TOKEN";

            CSRedisClientLock locker = null;
            while ((locker = client.Lock(lockKey, 10)) == null) ;

            var result = func();
            locker.Dispose();
            return result;
        }

        /// <summary>
        /// 检查是否需要自动提前延期。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="cacheKey"></param>
        /// <param name="factory"></param>
        /// <param name="expiration"></param>
        private void CheckPredictToDelay<T>(CSRedisClient client, string cacheKey, Func<T> factory, Func<ICacheItemExpiration> expiration = null)
        {
            if (Setting.AdvanceDelay == null)
            {
                return;
            }

            var expiry = GetExpirationTime(expiration);

            if (expiry != null)
            {
                //判断过期时间，如果小于指定的时间比例，则提前进行预存
                var liveTime = client.ObjectIdleTime(cacheKey);
                if (liveTime != null &&
                    (liveTime * 1000 / expiry.Value.TotalMilliseconds) <= Setting.AdvanceDelay.Value)
                {
                    Task.Run(() =>
                        {
                            var value = factory();
                            client.Set(cacheKey, Serialize(value), (int)expiry.Value.TotalSeconds);
                        });
                }
            }
        }

        /// <summary>
        /// 将数据放入到缓存服务器中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="cacheKey"></param>
        /// <param name="factory"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        private T PutToCache<T>(CSRedisClient client, string cacheKey, Func<T> factory, Func<ICacheItemExpiration> expiration = null)
        {
            var expiry = GetExpirationTime(expiration);
            var value = factory();

            client.Set(cacheKey, Serialize(value), expiry == null ? -1 : (int)expiry.Value.TotalSeconds);

            return value;
        }

        private void SetKeyExpiration(CSRedisClient client, string cacheKey, Func<ICacheItemExpiration> expiration)
        {
            var expiry = GetExpirationTime(expiration);
            if (expiry != null)
            {
                client.Expire(cacheKey, (int)expiry.Value.TotalSeconds);
            }
        }
#endif

        /// <summary>
        /// 获取缓存的有效期时间。
        /// </summary>
        /// <param name="expiration"></param>
        /// <returns></returns>
        private TimeSpan? GetExpirationTime(Func<ICacheItemExpiration> expiration)
        {
            return expiration == null ? null : GetExpirationTime(expiration());
        }

        /// <summary>
        /// 获取缓存的有效期时间。
        /// </summary>
        /// <param name="expiration"></param>
        /// <returns></returns>
        private TimeSpan? GetExpirationTime(ICacheItemExpiration expiration)
        {
            if (expiration is IExpirationTime relative)
            {
                return relative.Expiration;
            }

            return null;
        }
    }
}