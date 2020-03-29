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
using Fireasy.Common.Caching.Configuration;
using Fireasy.Common.Options;
using CSRedis;
using Microsoft.Extensions.Options;
#else
using StackExchange.Redis;
using System.Linq;
using System.Text.RegularExpressions;
#endif
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common;

namespace Fireasy.Redis
{
    /// <summary>
    /// 基于 Redis 的缓存管理器。
    /// </summary>
    [ConfigurationSetting(typeof(RedisConfigurationSetting))]
    public class CacheManager : RedisComponent, IEnhancedCacheManager
    {
        private static readonly SafetyDictionary<string, object> hashSet = new SafetyDictionary<string, object>();
        private Random random = new Random();

        /// <summary>
        /// 初始化 <see cref="CacheManager"/> 类的新实例。
        /// </summary>
        public CacheManager()
        {
        }

#if NETSTANDARD
        /// <summary>
        /// 初始化 <see cref="CacheManager"/> 类的新实例。
        /// </summary>
        /// <param name="ServiceProvider"></param>
        public CacheManager(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// 初始化 <see cref="CacheManager"/> 类的新实例。
        /// </summary>
        /// <param name="ServiceProvider"></param>
        /// <param name="options"></param>
        public CacheManager(IServiceProvider serviceProvider, IOptionsMonitor<RedisCachingOptions> options)
            : this(serviceProvider)
        {
            var optValue = options.CurrentValue;
            if (!optValue.IsConfigured())
            {
                return;
            }

            RedisConfigurationSetting setting;
            if (!string.IsNullOrEmpty(optValue.ConfigName))
            {
                var section = ConfigurationUnity.GetSection<CachingConfigurationSection>();
                if (section != null && section.GetSetting(optValue.ConfigName) is ExtendConfigurationSetting extSetting)
                {
                    setting = (RedisConfigurationSetting)extSetting.Extend;
                }
                else
                {
                    throw new InvalidOperationException($"无效的配置节: {optValue.ConfigName}。");
                }
            }
            else
            {
                setting = new RedisConfigurationSetting
                {
                    Password = optValue.Password,
                    ConnectionString = optValue.ConnectionString,
                    DefaultDb = optValue.DefaultDb,
                    DbRange = optValue.DbRange,
                    KeyRule = optValue.KeyRule,
                    ConnectTimeout = optValue.ConnectTimeout,
                    LockTimeout = optValue.LockTimeout,
                    SyncTimeout = optValue.SyncTimeout,
                    WriteBuffer = optValue.WriteBuffer,
                    PoolSize = optValue.PoolSize,
                    SerializerType = optValue.SerializerType,
                    Ssl = optValue.Ssl,
                    Twemproxy = optValue.Twemproxy,
                    SlidingTime = optValue.SlidingTime
                };

                RedisHelper.ParseHosts(setting, optValue.Hosts);
            }

            if (setting != null)
            {
                (this as IConfigurationSettingHostService).Attach(setting);
            }
        }
#endif

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
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            client.Set(cacheKey, Serialize(value), expire == null ? -1 : (int)expire.Value.TotalSeconds);
#else
            var db = GetDatabase(cacheKey);
            db.StringSet(cacheKey, Serialize(value), expire);
#endif
            return value;
        }

        /// <summary>
        /// 异步的，将对象插入到缓存管理器中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="value">要插入到缓存的对象。</param>
        /// <param name="expire">对象存放于缓存中的有效时间，到期后将从缓存中移除。如果此值为 null，则默认有效时间为 30 分钟。</param>
        /// <param name="removeCallback">当对象从缓存中移除时，使用该回调方法通知应用程序。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task<T> AddAsync<T>(string cacheKey, T value, TimeSpan? expire = null, CacheItemRemovedCallback removeCallback = null, CancellationToken cancellationToken = default)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            await client.SetAsync(cacheKey, Serialize(value), expire == null ? -1 : (int)expire.Value.TotalSeconds);
#else
            var db = GetDatabase(cacheKey);
            await db.StringSetAsync(cacheKey, Serialize(value), expire);
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
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var expiry = GetExpirationTime(expiration);

#if NETSTANDARD
            var client = GetConnection(cacheKey);
            client.Set(cacheKey, Serialize(value), expiry == null ? -1 : (int)expiry.Value.TotalSeconds);
#else
            var db = GetDatabase(cacheKey);
            db.StringSet(cacheKey, Serialize(value), expiry);
#endif
            return value;
        }

        /// <summary>
        /// 异步的，将对象插入到缓存管理器中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="value">要插入到缓存的对象。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="removeCallback">当对象从缓存中移除时，使用该回调方法通知应用程序。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task<T> AddAsync<T>(string cacheKey, T value, ICacheItemExpiration expiration, CacheItemRemovedCallback removeCallback = null, CancellationToken cancellationToken = default)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var expiry = GetExpirationTime(expiration);

#if NETSTANDARD
            var client = GetConnection(cacheKey);
            await client.SetAsync(cacheKey, Serialize(value), expiry == null ? -1 : (int)expiry.Value.TotalSeconds);
#else
            var db = GetDatabase(cacheKey);
            await db.StringSetAsync(cacheKey, Serialize(value), expiry);
#endif
            return value;
        }

        /// <summary>
        /// 清除所有缓存。
        /// </summary>
        public void Clear()
        {
#if NETSTANDARD
            foreach (var client in GetConnections())
            {
                client.Del(client.Keys("*"));
            }
#else
            var client = GetConnection();
            foreach (var endpoint in client.GetEndPoints())
            {
                var server = client.GetServer(endpoint);
                foreach (var db in GetDatabases())
                {
                    foreach (var key in server.Keys(db.Database))
                    {
                        db.KeyDelete(key);
                    }
                }
            }
#endif

            hashSet.Clear();
        }

        /// <summary>
        /// 异步的，清除所有缓存。
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// </summary>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
#if NETSTANDARD
            foreach (var client in GetConnections())
            {
                await client.DelAsync(client.Keys("*"));
            }
#else
            var client = GetConnection();
            foreach (var endpoint in client.GetEndPoints())
            {
                var server = client.GetServer(endpoint);
                foreach (var db in GetDatabases())
                {
                    foreach (var key in server.Keys(db.Database))
                    {
                        await db.KeyDeleteAsync(key);
                    }
                }
            }
#endif

            hashSet.Clear();
        }

        /// <summary>
        /// 确定缓存中是否包含指定的缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>如果缓存中包含指定缓存键的对象，则为 true，否则为 false。</returns>
        public bool Contains(string cacheKey)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            return client.Exists(cacheKey);
#else
            var db = GetDatabase(cacheKey);
            return db.KeyExists(cacheKey);
#endif
        }


        /// <summary>
        /// 异步的，确定缓存中是否包含指定的缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果缓存中包含指定缓存键的对象，则为 true，否则为 false。</returns>
        public async Task<bool> ContainsAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            return await client.ExistsAsync(cacheKey);
#else
            var db = GetDatabase(cacheKey);
            return await db.KeyExistsAsync(cacheKey);
#endif
        }

        /// <summary>
        /// 获取缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns></returns>
        public TimeSpan? GetExpirationTime(string cacheKey)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            var time = client.Ttl(cacheKey);
            return time == 0 ? (TimeSpan?)null : TimeSpan.FromSeconds(time);
#else
            var db = GetDatabase(cacheKey);
            return db.KeyTimeToLive(cacheKey);
#endif
        }

        /// <summary>
        /// 异步的，获取缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<TimeSpan?> GetExpirationTimeAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            var time = await client.TtlAsync(cacheKey);
            return time == 0 ? (TimeSpan?)null : TimeSpan.FromSeconds(time);
#else
            var db = GetDatabase(cacheKey);
            return await db.KeyTimeToLiveAsync(cacheKey);
#endif
        }

        /// <summary>
        /// 设置缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        public void SetExpirationTime(string cacheKey, Func<ICacheItemExpiration> expiration)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            SetKeyExpiration(client, cacheKey, expiration);
#else
            var db = GetDatabase(cacheKey);
            SetKeyExpiration(db, cacheKey, expiration);
#endif
        }

        /// <summary>
        /// 异步的，设置缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task SetExpirationTimeAsync(string cacheKey, Func<ICacheItemExpiration> expiration, CancellationToken cancellationToken = default)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            await SetKeyExpirationAsync(client, cacheKey, expiration);
#else
            var db = GetDatabase(cacheKey);
            await SetKeyExpirationAsync(db, cacheKey, expiration);
#endif
        }

        /// <summary>
        /// 获取缓存中指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        public object Get(string cacheKey)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            var value = client.Get(cacheKey);
#else
            var db = GetDatabase(cacheKey);
            var value = db.StringGet(cacheKey);
#endif
            if (!string.IsNullOrEmpty(value))
            {
                return Deserialize<dynamic>(value);
            }

            return null;
        }

        /// <summary>
        /// 异步的，获取缓存中指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        public async Task<object> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            var value = await client.GetAsync(cacheKey);
#else
            var db = GetDatabase(cacheKey);
            var value = await db.StringGetAsync(cacheKey);
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
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            var value = client.Get(cacheKey);
#else
            var db = GetDatabase(cacheKey);
            var value = db.StringGet(cacheKey);
#endif
            if (!string.IsNullOrEmpty(value))
            {
                return (T)Deserialize(typeof(T), value);
            }

            return default(T);
        }

        /// <summary>
        /// 异步的，获取缓存中指定缓存键的对象。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        public async Task<T> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            var value = await client.GetAsync(cacheKey);
#else
            var db = GetDatabase(cacheKey);
            var value = await db.StringGetAsync(cacheKey);
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
        /// <param name="pattern"></param>
        /// <returns></returns>
        public IEnumerable<string> GetKeys(string pattern)
        {
            var keys = new List<string>();
#if NETSTANDARD
            foreach (var client in GetConnections())
            {
                keys.AddRange(client.Keys(pattern));
            }

#else
            var client = GetConnection();
            foreach (var endpoint in client.GetEndPoints())
            {
                var server = client.GetServer(endpoint);
                foreach (var db in GetDatabases())
                {
                    keys.AddRange(server.Keys(db.Database).Cast<string>());
                }
            }
#endif
            return keys;
        }

        /// <summary>
        /// 异步的，获取所有的 key。
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetKeysAsync(string pattern, CancellationToken cancellationToken = default)
        {
#if NETSTANDARD
            var keys = new List<string>();
            foreach (var client in GetConnections())
            {
                keys.AddRange(await client.KeysAsync(pattern));
            }

            return keys;
#else
            return GetKeys(pattern);
#endif
        }

        /// <summary>
        /// 从缓存中移除指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        public void Remove(string cacheKey)
        {
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            var success = client.Del(cacheKey) > 0;
#else
            var db = GetDatabase(cacheKey);
            var success = db.KeyDelete(cacheKey);
#endif

            if (success && hashSet.ContainsKey(cacheKey))
            {
                hashSet.TryRemove(cacheKey, out object _);
            }
        }

        /// <summary>
        /// 异步的，从缓存中移除指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            var success = await client.DelAsync(cacheKey) > 0;
#else
            var db = GetDatabase(cacheKey);
            var success = await db.KeyDeleteAsync(cacheKey);
#endif

            if (success && hashSet.ContainsKey(cacheKey))
            {
                hashSet.TryRemove(cacheKey, out object _);
            }
        }

        /// <summary>
        /// 尝试获取指定缓存键的对象，如果没有则使用函数添加对象到缓存中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="valueCreator">用于添加缓存对象的函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public T TryGet<T>(string cacheKey, Func<T> valueCreator, Func<ICacheItemExpiration> expiration = null)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            try
            {
                CheckCache<T> GetCacheValue()
                {
                    if (client.Exists(cacheKey))
                    {
                        var redisValue = client.Get(cacheKey);
                        if (!string.IsNullOrEmpty(redisValue))
                        {
                            HandleSlidingTime(client, cacheKey);

                            return CheckCache<T>.Result(Deserialize<T>(redisValue));
                        }
                    }

                    return CheckCache<T>.Null();
                }

                var ck = GetCacheValue();
                if (ck.HasValue)
                {
                    return ck.Value;
                }

                return RedisHelper.Lock(client, GetLockToken(cacheKey), Setting.LockTimeout, () =>
                    {
                        var ck1 = GetCacheValue();
                        if (ck1.HasValue)
                        {
                            return ck1.Value;
                        }

                        var expiry = GetExpirationTime(expiration);
                        var value = valueCreator();

                        client.Set(cacheKey, base.Serialize(value), expiry == null ? -1 : (int)expiry.Value.TotalSeconds);

                        return value;
                    });
            }
            catch (Exception exp)
            {
                Tracer.Error($"RedisCache TryGetValue throw exception:\n{exp.Output()}");
                return valueCreator();
            }
#else
            var db = GetDatabase(cacheKey);
            try
            {
                CheckCache<T> GetCacheValue()
                {
                    if (db.KeyExists(cacheKey))
                    {
                        var redisValue = db.StringGet(cacheKey);
                        if (!string.IsNullOrEmpty(redisValue))
                        {
                            HandleSlidingTime(db, cacheKey);

                            return CheckCache<T>.Result(Deserialize<T>(redisValue));
                        }
                    }

                    return CheckCache<T>.Null();
                }

                var ck = GetCacheValue();
                if (ck.HasValue)
                {
                    return ck.Value;
                }

                return RedisHelper.Lock(db, cacheKey, Setting.LockTimeout, () =>
                    {
                        var ck1 = GetCacheValue();
                        if (ck1.HasValue)
                        {
                            return ck1.Value;
                        }

                        var expiry = GetExpirationTime(expiration);
                        var value = valueCreator();

                        db.StringSet(cacheKey, Serialize(value), expiry);

                        return value;
                    });
            }
            catch (Exception exp)
            {
                Tracer.Error($"RedisCache TryGetValue throw exception:\n{exp.Output()}");
                return valueCreator();
            }
#endif
        }

        /// <summary>
        /// 异步的，尝试获取指定缓存键的对象，如果没有则使用函数添加对象到缓存中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="valueCreator">用于添加缓存对象的函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<T> TryGetAsync<T>(string cacheKey, Func<Task<T>> valueCreator, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            try
            {
                async Task<CheckCache<T>> GetCacheValue()
                {
                    if (await client.ExistsAsync(cacheKey))
                    {
                        var redisValue = await client.GetAsync(cacheKey);
                        if (!string.IsNullOrEmpty(redisValue))
                        {
                            await HandleSlidingTimeAsync(client, cacheKey);

                            return CheckCache<T>.Result(Deserialize<T>(redisValue));
                        }
                    }

                    return CheckCache<T>.Null();
                }

                var ck = await GetCacheValue();
                if (ck.HasValue)
                {
                    return ck.Value;
                }

                return await RedisHelper.LockAsync(client, GetLockToken(cacheKey), Setting.LockTimeout, async () =>
                    {
                        var ck1 = await GetCacheValue();
                        if (ck1.HasValue)
                        {
                            return ck1.Value;
                        }

                        var expiry = GetExpirationTime(expiration);
                        var value = await valueCreator();

                        await client.SetAsync(cacheKey, Serialize(value), expiry == null ? -1 : (int)expiry.Value.TotalSeconds);

                        return value;
                    });
            }
            catch (Exception exp)
            {
                Tracer.Error($"RedisCache TryGetValue throw exception:\n{exp.Output()}");
                return await valueCreator();
            }
#else
            var db = GetDatabase(cacheKey);
            try
            {
                async Task<CheckCache<T>> GetCacheValue()
                {
                    if (await db.KeyExistsAsync(cacheKey))
                    {
                        var redisValue = await db.StringGetAsync(cacheKey);
                        if (!string.IsNullOrEmpty(redisValue))
                        {
                            await HandleSlidingAsync(db, cacheKey);

                            return CheckCache<T>.Result(Deserialize<T>(redisValue));
                        }
                    }

                    return CheckCache<T>.Null();
                }

                var ck = await GetCacheValue();
                if (ck.HasValue)
                {
                    return ck.Value;
                }

                return await RedisHelper.LockAsync(db, cacheKey, Setting.LockTimeout, async () =>
                    {
                        var ck1 = await GetCacheValue();
                        if (ck1.HasValue)
                        {
                            return ck1.Value;
                        }

                        var expiry = GetExpirationTime(expiration);
                        var value = await valueCreator();

                        await db.StringSetAsync(cacheKey, Serialize(value), expiry);

                        return value;
                    });
            }
            catch (Exception exp)
            {
                Tracer.Error($"RedisCache TryGetValue throw exception:\n{exp.Output()}");
                return await valueCreator();
            }
#endif
        }

        /// <summary>
        /// 尝试获取指定缓存键的对象，如果没有则使用函数添加对象到缓存中。
        /// </summary>
        /// <param name="dataType">数据类型。</param>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="valueCreator">用于添加缓存对象的函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public object TryGet(Type dataType, string cacheKey, Func<object> valueCreator, Func<ICacheItemExpiration> expiration = null)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            try
            {
                object GetCacheValue()
                {
                    if (client.Exists(cacheKey))
                    {
                        var redisValue = client.Get(cacheKey);
                        if (!string.IsNullOrEmpty(redisValue))
                        {
                            HandleSlidingTime(client, cacheKey);

                            return Deserialize(dataType, redisValue);
                        }
                    }

                    return null;
                }

                var value = GetCacheValue();
                if (value != null)
                {
                    return value;
                }

                return RedisHelper.Lock(client, GetLockToken(cacheKey), Setting.LockTimeout, () =>
                    {
                        value = GetCacheValue();
                        if (value != null)
                        {
                            return value;
                        }

                        var expiry = GetExpirationTime(expiration);
                        value = valueCreator();

                        client.Set(cacheKey, Serialize(value), expiry == null ? -1 : (int)expiry.Value.TotalSeconds);

                        return value;
                    });
            }
            catch (Exception exp)
            {
                Tracer.Error($"RedisCache TryGetValue throw exception:\n{exp.Output()}");
                return valueCreator();
            }
#else
            var db = GetDatabase(cacheKey);
            try
            {
                object GetCacheValue()
                {
                    if (db.KeyExists(cacheKey))
                    {
                        var redisValue = db.StringGet(cacheKey);
                        if (!string.IsNullOrEmpty(redisValue))
                        {
                            HandleSlidingTime(db, cacheKey);

                            return Deserialize(dataType, redisValue);
                        }
                    }

                    return null;
                }

                var value = GetCacheValue();
                if (value != null)
                {
                    return value;
                }

                return RedisHelper.Lock(db, cacheKey, Setting.LockTimeout, () =>
                    {
                        value = GetCacheValue();
                        if (value != null)
                        {
                            return value;
                        }

                        var expiry = GetExpirationTime(expiration);
                        value = valueCreator();

                        db.StringSet(cacheKey, Serialize(value), expiry);

                        return value;
                    });
            }
            catch (Exception exp)
            {
                Tracer.Error($"RedisCache TryGetValue throw exception:\n{exp.Output()}");
                return valueCreator();
            }
#endif
        }

        /// <summary>
        /// 异步的，尝试获取指定缓存键的对象，如果没有则使用函数添加对象到缓存中。
        /// </summary>
        /// <param name="dataType">数据类型。</param>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="valueCreator">用于添加缓存对象的函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<object> TryGetAsync(Type dataType, string cacheKey, Func<Task<object>> valueCreator, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            try
            {
                async Task<object> GetCacheValue()
                {
                    if (await client.ExistsAsync(cacheKey))
                    {
                        var redisValue = await client.GetAsync(cacheKey);
                        if (!string.IsNullOrEmpty(redisValue))
                        {
                            await HandleSlidingTimeAsync(client, cacheKey);

                            return Deserialize(dataType, redisValue);
                        }
                    }

                    return null;
                }

                var value = await GetCacheValue();
                if (value != null)
                {
                    return value;
                }

                return await RedisHelper.LockAsync(client, GetLockToken(cacheKey), Setting.LockTimeout, async () =>
                    {
                        value = GetCacheValue();
                        if (value != null)
                        {
                            return value;
                        }

                        var expiry = GetExpirationTime(expiration);
                        value = await valueCreator();

                        await client.SetAsync(cacheKey, Serialize(value), expiry == null ? -1 : (int)expiry.Value.TotalSeconds);

                        return value;
                    });
            }
            catch (Exception exp)
            {
                Tracer.Error($"RedisCache TryGetValue throw exception:\n{exp.Output()}");
                return await valueCreator();
            }
#else
            var db = GetDatabase(cacheKey);
            try
            {
                async Task<object> GetCacheValue()
                {
                    if (await db.KeyExistsAsync(cacheKey))
                    {
                        var redisValue = await db.StringGetAsync(cacheKey);
                        if (!string.IsNullOrEmpty(redisValue))
                        {
                            await HandleSlidingAsync(db, cacheKey);

                            return Deserialize(dataType, redisValue);
                        }
                    }

                    return null;
                }

                var value = await GetCacheValue();
                if (value != null)
                {
                    return value;
                }

                return await RedisHelper.LockAsync(db, cacheKey, Setting.LockTimeout, async () =>
                    {
                        value = await GetCacheValue();
                        if (value != null)
                        {
                            return value;
                        }

                        var expiry = GetExpirationTime(expiration);
                        value = await valueCreator();

                        await db.StringSetAsync(cacheKey, Serialize(value), expiry);

                        return value;
                    });
            }
            catch (Exception exp)
            {
                Tracer.Error($"RedisCache TryGetValue throw exception:\n{exp.Output()}");
                return await valueCreator();
            }
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
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
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
            var db = GetDatabase(cacheKey);
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

            value = default;
            return false;
        }

        /// <summary>
        /// 尝试获取增量。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="valueCreator">用于初始化数据的函数。</param>
        /// <param name="step">递增的步数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public long TryIncrement(string cacheKey, Func<long> valueCreator, int step = 1, Func<ICacheItemExpiration> expiration = null)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            return RedisHelper.Lock(client, GetLockToken(cacheKey), Setting.LockTimeout, () =>
                {
                    long ret = 0;
                    if (client.Exists(cacheKey))
                    {
                        ret = client.IncrBy(cacheKey, step);
                    }
                    else
                    {
                        ret = client.IncrBy(cacheKey, valueCreator() + step);

                        SetKeyExpiration(client, cacheKey, expiration);
                    }

                    return ret;
                });
#else
            var db = GetDatabase(cacheKey);
            return RedisHelper.Lock(db, cacheKey, Setting.LockTimeout, () =>
                {
                    long ret = 0;
                    if (db.KeyExists(cacheKey))
                    {
                        ret = db.StringIncrement(cacheKey, step);
                    }
                    else
                    {
                        ret = db.StringIncrement(cacheKey, valueCreator() + step);

                        SetKeyExpiration(db, cacheKey, expiration);
                    }

                    return ret;
                });
#endif
        }

        /// <summary>
        /// 异步的，尝试获取增量。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="valueCreator">用于初始化数据的函数。</param>
        /// <param name="step">递增的步数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<long> TryIncrementAsync(string cacheKey, Func<long> valueCreator, int step = 1, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            return await RedisHelper.LockAsync(client, GetLockToken(cacheKey), Setting.LockTimeout, async () =>
                {
                    long ret = 0;
                    if (await client.ExistsAsync(cacheKey))
                    {
                        ret = await client.IncrByAsync(cacheKey, step);
                    }
                    else
                    {
                        ret = await client.IncrByAsync(cacheKey, valueCreator() + step);

                        await SetKeyExpirationAsync(client, cacheKey, expiration);
                    }

                    return ret;
                });
#else
            var db = GetDatabase(cacheKey);
            return await RedisHelper.LockAsync(db, GetLockToken(cacheKey), Setting.LockTimeout, async () =>
                {
                    long ret = 0;
                    if (await db.KeyExistsAsync(cacheKey))
                    {
                        ret = await db.StringIncrementAsync(cacheKey, step);
                    }
                    else
                    {
                        ret = await db.StringIncrementAsync(cacheKey, valueCreator() + step);

                        await SetKeyExpirationAsync(db, cacheKey, expiration);
                    }

                    return ret;
                });
#endif
        }

        /// <summary>
        /// 尝试获取减量。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="valueCreator">用于初始化数据的函数。</param>
        /// <param name="step">递减的步数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public long TryDecrement(string cacheKey, Func<long> valueCreator, int step = 1, Func<ICacheItemExpiration> expiration = null)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            return RedisHelper.Lock(client, GetLockToken(cacheKey), Setting.LockTimeout, () =>
                {
                    long ret = 0;
                    if (client.Exists(cacheKey))
                    {
                        ret = client.IncrBy(cacheKey, -step);
                    }
                    else
                    {
                        client.IncrBy(cacheKey, valueCreator());
                        ret = client.IncrBy(cacheKey, -step);

                        SetKeyExpiration(client, cacheKey, expiration);
                    }

                    return ret;
                });
#else
            var db = GetDatabase(cacheKey);
            return RedisHelper.Lock(db, GetLockToken(cacheKey), Setting.LockTimeout, () =>
                {
                    long ret = 0;
                    if (db.KeyExists(cacheKey))
                    {
                        ret = db.StringDecrement(cacheKey, step);
                    }
                    else
                    {
                        db.StringIncrement(cacheKey, valueCreator());
                        ret = db.StringDecrement(cacheKey, step);

                        SetKeyExpiration(db, cacheKey, expiration);
                    }

                    return ret;
                });
#endif
        }

        /// <summary>
        /// 异步的，尝试获取减量。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="valueCreator">用于初始化数据的函数。</param>
        /// <param name="step">递减的步数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<long> TryDecrementAsync(string cacheKey, Func<long> valueCreator, int step = 1, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
#if NETSTANDARD
            var client = GetConnection(cacheKey);
            return await RedisHelper.LockAsync(client, GetLockToken(cacheKey), Setting.LockTimeout, async () =>
                {
                    long ret = 0;
                    if (await client.ExistsAsync(cacheKey))
                    {
                        ret = await client.IncrByAsync(cacheKey, -step);
                    }
                    else
                    {
                        await client.IncrByAsync(cacheKey, valueCreator());
                        ret = await client.IncrByAsync(cacheKey, -step);

                        await SetKeyExpirationAsync(client, cacheKey, expiration);
                    }

                    return ret;
                });
#else
            var db = GetDatabase(cacheKey);
            return await RedisHelper.LockAsync(db, GetLockToken(cacheKey), Setting.LockTimeout, async () =>
                {
                    long ret = 0;
                    if (await db.KeyExistsAsync(cacheKey))
                    {
                        ret = await db.StringDecrementAsync(cacheKey, step);
                    }
                    else
                    {
                        db.StringIncrement(cacheKey, valueCreator());
                        ret = db.StringDecrement(cacheKey, step);

                        await SetKeyExpirationAsync(db, cacheKey, expiration);
                    }

                    return ret;
                });
#endif
        }

        /// <summary>
        /// 获取一个哈希集。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="cacheKey">用于哈希集的缓存键。</param>
        /// <param name="initializeSet">用于初始化哈希集的函数。</param>
        /// <param name="checkExpiration">是否检查元素的过期时间。</param>
        /// <returns></returns>
        public ICacheHashSet<TKey, TValue> GetHashSet<TKey, TValue>(string cacheKey, Func<IEnumerable<Tuple<TKey, TValue, ICacheItemExpiration>>> initializeSet = null, bool checkExpiration = true)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);

            try
            {
#if NETSTANDARD
                var client = GetConnection(cacheKey);
                return (ICacheHashSet<TKey, TValue>)hashSet.GetOrAdd(cacheKey, () =>
                        new RedisHashSet<TKey, TValue>(Setting, cacheKey, initializeSet, client, v => Serialize(v), s => Deserialize<RedisCacheItem<TValue>>(s), checkExpiration)
                    );
#else
                var db = GetDatabase(cacheKey);
                return (ICacheHashSet<TKey, TValue>)hashSet.GetOrAdd(cacheKey, () =>
                        new RedisHashSet<TKey, TValue>(Setting, cacheKey, initializeSet, db, v => Serialize(v), s => Deserialize<RedisCacheItem<TValue>>(s), checkExpiration)
                    );
#endif
            }
            catch (Exception exp)
            {
                throw;
            }
        }


        /// <summary>
        /// 使用事务。
        /// </summary>
        /// <param name="action"></param>
        public void UseTransaction(string token, Action action, TimeSpan timeout)
        {
            if (RedisTransactionContext.Current != null)
            {
                action();
                return;
            }

            using var scope = new RedisTransactionContext();
            token = ServiceProvider.GetCacheKey(token);
#if NETSTANDARD
            RedisHelper.Lock(GetConnection(token), token, timeout, action);
#else
            RedisHelper.Lock(GetDatabase(token), token, timeout, action);
#endif
        }

        /// <summary>
        /// 异步的，使用事务。
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public async Task UseTransactionAsync(string token, Func<Task> func, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (RedisTransactionContext.Current != null)
            {
                await func();
                return;
            }

            using var scope = new RedisTransactionContext();
            token = ServiceProvider.GetCacheKey(token);
#if NETSTANDARD
            await RedisHelper.LockAsync(GetConnection(token), token, timeout, func);
#else
            await RedisHelper.LockAsync(GetDatabase(token), token, timeout, func);
#endif
        }


#if NETSTANDARD
        /// <summary>
        /// 处理滑行时间。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cacheKey"></param>
        private void HandleSlidingTime(CSRedisClient client, string cacheKey)
        {
            if (Setting.SlidingTime == TimeSpan.Zero)
            {
                return;
            }

            var time = client.Ttl(cacheKey);
            if (time == 0 || time > 60)
            {
                return;
            }

            var liveTime = Setting.SlidingTime.Add(TimeSpan.FromSeconds(time));
            SetExpirationTime(cacheKey, () => new RelativeTime(liveTime));
        }

        /// <summary>
        /// 处理滑行时间。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cacheKey"></param>
        private async Task HandleSlidingTimeAsync(CSRedisClient client, string cacheKey)
        {
            if (Setting.SlidingTime == TimeSpan.Zero)
            {
                return;
            }

            var time = await client.TtlAsync(cacheKey);
            if (time == 0 || time > 60)
            {
                return;
            }

            var liveTime = Setting.SlidingTime.Add(TimeSpan.FromSeconds(time));
            await SetExpirationTimeAsync(cacheKey, () => new RelativeTime(liveTime));
        }

        private void SetKeyExpiration(CSRedisClient client, string cacheKey, Func<ICacheItemExpiration> expiration)
        {
            if (!client.Exists(cacheKey))
            {
                return;
            }

            var expiry = GetExpirationTime(expiration);
            if (expiry != null)
            {
                client.Expire(cacheKey, (int)expiry.Value.TotalSeconds);
            }
            else
            {
                client.Expire(cacheKey, TimeSpan.MaxValue);
            }
        }

        private async Task SetKeyExpirationAsync(CSRedisClient client, string cacheKey, Func<ICacheItemExpiration> expiration)
        {
            if (!(await client.ExistsAsync(cacheKey)))
            {
                return;
            }

            var expiry = GetExpirationTime(expiration);
            if (expiry != null)
            {
                await client.ExpireAsync(cacheKey, (int)expiry.Value.TotalSeconds);
            }
            else
            {
                await client.ExpireAsync(cacheKey, TimeSpan.MaxValue);
            }
        }
#else
        /// <summary>
        /// 处理滑行时间。
        /// </summary>
        /// <param name="db"></param>
        /// <param name="cacheKey"></param>
        private void HandleSlidingTime(IDatabase db, string cacheKey)
        {
            if (Setting.SlidingTime == TimeSpan.Zero)
            {
                return;
            }

            var time = db.KeyTimeToLive(cacheKey);
            if (time == null || time.Value.TotalSeconds > 60)
            {
                return;
            }

            var liveTime = time.Value + Setting.SlidingTime;
            SetExpirationTime(cacheKey, () => new RelativeTime(liveTime));
        }

        /// <summary>
        /// 处理滑行时间。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="cacheKey"></param>
        private async Task HandleSlidingAsync(IDatabase db, string cacheKey)
        {
            if (Setting.SlidingTime == TimeSpan.Zero)
            {
                return;
            }

            var time = await db.KeyTimeToLiveAsync(cacheKey);
            if (time == null || time.Value.TotalSeconds > 60)
            {
                return;
            }

            var liveTime = time.Value + Setting.SlidingTime;
            await SetExpirationTimeAsync(cacheKey, () => new RelativeTime(liveTime));
        }

        private void SetKeyExpiration(IDatabase db, string cacheKey, Func<ICacheItemExpiration> expiration)
        {
            if (!db.KeyExists(cacheKey))
            {
                return;
            }

            var expiry = GetExpirationTime(expiration);
            if (expiry != null)
            {
                db.KeyExpire(cacheKey, expiry);
            }
            else
            {
                db.KeyExpire(cacheKey, (TimeSpan?)null);
            }
        }

        private async Task SetKeyExpirationAsync(IDatabase db, string cacheKey, Func<ICacheItemExpiration> expiration)
        {
            if (!(await db.KeyExistsAsync(cacheKey)))
            {
                return;
            }

            var expiry = GetExpirationTime(expiration);
            if (expiry != null)
            {
                await db.KeyExpireAsync(cacheKey, expiry);
            }
            else
            {
                await db.KeyExpireAsync(cacheKey, (TimeSpan?)null);
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
                var sec = random.Next(0, 10);
                return relative.Expiration.Add(TimeSpan.FromSeconds(sec));
            }

            return null;
        }

        private string GetLockToken(string key)
        {
            return $"{key}:LOCK_TOKEN";
        }
    }
}