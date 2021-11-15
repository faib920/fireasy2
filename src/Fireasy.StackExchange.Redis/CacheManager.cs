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
using Microsoft.Extensions.Options;
#endif
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common;
using System.Linq;

namespace Fireasy.Redis
{
    /// <summary>
    /// 基于 Redis 的缓存管理器。
    /// </summary>
    [ConfigurationSetting(typeof(RedisConfigurationSetting))]
    public class CacheManager : RedisComponent, IDistributedCacheManager, IEnhancedCacheManager
    {
        private static readonly SafetyDictionary<string, object> _hashSet = new SafetyDictionary<string, object>();

        /// <summary>
        /// 初始化 <see cref="CacheManager"/> 类的新实例。
        /// </summary>
        public CacheManager()
            : base()
        {
        }

        /// <summary>
        /// 初始化 <see cref="CacheManager"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        public CacheManager(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

#if NETSTANDARD
        /// <summary>
        /// 初始化 <see cref="CacheManager"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="options"></param>
        public CacheManager(IServiceProvider serviceProvider, IOptionsMonitor<RedisCachingOptions> options)
            : base(serviceProvider)
        {
            RedisConfigurationSetting setting = null;
            var optValue = options.CurrentValue;
            if (!optValue.IsConfigured())
            {
                var section = ConfigurationUnity.GetSection<CachingConfigurationSection>();
                var matchSetting = section.Settings.FirstOrDefault(s => s.Value.CacheType == typeof(CacheManager)).Value;
                if (matchSetting != null && section.GetSetting(matchSetting.Name) is ExtendConfigurationSetting extSetting)
                {
                    setting = (RedisConfigurationSetting)extSetting.Extend;
                }
                else
                {
                    throw new InvalidOperationException($"未发现与 {typeof(CacheManager).FullName} 相匹配的配置。");
                }
            }
            else
            {
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
                        SerializerType = optValue.SerializerType,
                        Ssl = optValue.Ssl,
                        SlidingTime = optValue.SlidingTime,
                        Prefix = optValue.Prefix,
                        IgnoreException = optValue.IgnoreException,
                        Twemproxy = optValue.Twemproxy,
                        MinIoThreads = optValue.MinIoThreads
                    };

                    RedisHelper.ParseHosts(setting, optValue.Hosts);
                }
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
            var db = GetDatabase(cacheKey);
            db.StringSet(cacheKey, Serialize(value), expire);
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
            cancellationToken.ThrowIfCancellationRequested();

            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            await db.StringSetAsync(cacheKey, Serialize(value), expire);
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

            var db = GetDatabase(cacheKey);
            db.StringSet(cacheKey, Serialize(value), expiry);
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
            cancellationToken.ThrowIfCancellationRequested();

            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var expiry = GetExpirationTime(expiration);

            var db = GetDatabase(cacheKey);
            await db.StringSetAsync(cacheKey, Serialize(value), expiry);
            return value;
        }

        /// <summary>
        /// 清除所有缓存。
        /// </summary>
        public void Clear()
        {
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

            _hashSet.Clear();
        }

        /// <summary>
        /// 异步的，清除所有缓存。
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// </summary>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            _hashSet.Clear();
        }

        /// <summary>
        /// 确定缓存中是否包含指定的缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>如果缓存中包含指定缓存键的对象，则为 true，否则为 false。</returns>
        public bool Contains(string cacheKey)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            return db.KeyExists(cacheKey);
        }


        /// <summary>
        /// 异步的，确定缓存中是否包含指定的缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果缓存中包含指定缓存键的对象，则为 true，否则为 false。</returns>
        public async Task<bool> ContainsAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            return await db.KeyExistsAsync(cacheKey);
        }

        /// <summary>
        /// 获取缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns></returns>
        public TimeSpan? GetExpirationTime(string cacheKey)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            return db.KeyTimeToLive(cacheKey);
        }

        /// <summary>
        /// 异步的，获取缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<TimeSpan?> GetExpirationTimeAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            return await db.KeyTimeToLiveAsync(cacheKey);
        }

        /// <summary>
        /// 设置缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        public void SetExpirationTime(string cacheKey, Func<ICacheItemExpiration> expiration)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            SetKeyExpiration(db, cacheKey, expiration);
        }

        /// <summary>
        /// 异步的，设置缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task SetExpirationTimeAsync(string cacheKey, Func<ICacheItemExpiration> expiration, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            await SetKeyExpirationAsync(db, cacheKey, expiration);
        }

        /// <summary>
        /// 获取缓存中指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        public object Get(string cacheKey)
        {
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            string value = db.StringGet(cacheKey);
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
            cancellationToken.ThrowIfCancellationRequested();

            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            string value = await db.StringGetAsync(cacheKey);
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
            var db = GetDatabase(cacheKey);
            string value = db.StringGet(cacheKey);
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
            cancellationToken.ThrowIfCancellationRequested();

            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            string value = await db.StringGetAsync(cacheKey);
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
            var client = GetConnection();
            foreach (var endpoint in client.GetEndPoints())
            {
                var server = client.GetServer(endpoint);
                foreach (var db in GetDatabases())
                {
                    foreach (var key in server.Keys(db.Database, pattern))
                    {
                        keys.Add(key);
                    }
                }
            }

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
            cancellationToken.ThrowIfCancellationRequested();
            var keys = new List<string>();
            var client = GetConnection();
            foreach (var endpoint in client.GetEndPoints())
            {
                var server = client.GetServer(endpoint);
                foreach (var db in GetDatabases())
                {
                    await foreach (string key in server.KeysAsync(db.Database, pattern))
                    {
                        keys.Add(key);
                    }
                }
            }

            return keys;
        }

        /// <summary>
        /// 从缓存中移除指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        public void Remove(string cacheKey)
        {
            var db = GetDatabase(cacheKey);
            var success = db.KeyDelete(cacheKey);

            if (success && _hashSet.ContainsKey(cacheKey))
            {
                _hashSet.TryRemove(cacheKey, out object _);
            }
        }

        /// <summary>
        /// 异步的，从缓存中移除指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            var success = await db.KeyDeleteAsync(cacheKey);

            if (success && _hashSet.ContainsKey(cacheKey))
            {
                _hashSet.TryRemove(cacheKey, out object _);
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
            var db = GetDatabase(cacheKey);
            try
            {
                string redisValue = db.StringGet(cacheKey);
                if (!string.IsNullOrEmpty(redisValue))
                {
                    HandleSlidingTime(db, cacheKey);

                    return Deserialize<T>(redisValue);
                }

                var expiry = GetExpirationTime(expiration);
                var value = valueCreator();

                db.StringSet(cacheKey, Serialize(value), expiry);

                return value;
            }
            catch (IOException exp)
            {
                if (Setting.IgnoreException)
                {
                    Tracer.Error($"RedisCache TryGetValue throw exception:\n{exp.Output()}");
                    return valueCreator();
                }

                throw exp;
            }
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
            cancellationToken.ThrowIfCancellationRequested();

            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            try
            {
                string redisValue = await db.StringGetAsync(cacheKey);
                if (!string.IsNullOrEmpty(redisValue))
                {
                    await HandleSlidingTimeAsync(db, cacheKey);

                    return Deserialize<T>(redisValue);
                }

                var expiry = GetExpirationTime(expiration);
                var value = await valueCreator();

                await db.StringSetAsync(cacheKey, Serialize(value), expiry);

                return value;
            }
            catch (IOException exp)
            {
                if (Setting.IgnoreException)
                {
                    Tracer.Error($"RedisCache TryGetValue throw exception:\n{exp.Output()}");
                    return await valueCreator();
                }

                throw exp;
            }
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
            var db = GetDatabase(cacheKey);
            try
            {
                string redisValue = db.StringGet(cacheKey);
                if (!string.IsNullOrEmpty(redisValue))
                {
                    HandleSlidingTime(db, cacheKey);

                    return Deserialize(dataType, redisValue);
                }

                var expiry = GetExpirationTime(expiration);
                var value = valueCreator();

                db.StringSet(cacheKey, Serialize(value), expiry);

                return value;
            }
            catch (IOException exp)
            {
                if (Setting.IgnoreException)
                {
                    Tracer.Error($"RedisCache TryGetValue throw exception:\n{exp.Output()}");
                    return valueCreator();
                }

                throw exp;
            }
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
            cancellationToken.ThrowIfCancellationRequested();

            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            try
            {
                string redisValue = await db.StringGetAsync(cacheKey);
                if (!string.IsNullOrEmpty(redisValue))
                {
                    await HandleSlidingTimeAsync(db, cacheKey);

                    return Deserialize(dataType, redisValue);
                }

                var expiry = GetExpirationTime(expiration);
                var value = await valueCreator();

                await db.StringSetAsync(cacheKey, Serialize(value), expiry);

                return value;
            }
            catch (IOException exp)
            {
                if (Setting.IgnoreException)
                {
                    Tracer.Error($"RedisCache TryGetValue throw exception:\n{exp.Output()}");
                    return await valueCreator();
                }

                throw exp;
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
            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            string redisValue = db.StringGet(cacheKey);
            if (!string.IsNullOrEmpty(redisValue))
            {
                value = Deserialize<T>(redisValue);
                return true;
            }

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
            var db = GetDatabase(cacheKey);
            return RedisHelper.Lock(db, GetLockToken(cacheKey), Setting.LockTimeout, () =>
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
            cancellationToken.ThrowIfCancellationRequested();

            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            return await RedisHelper.LockAsync(db, GetLockToken(cacheKey), Setting.LockTimeout, async () =>
                {
                    long ret = 0;
                    if (await db.KeyExistsAsync(cacheKey).ConfigureAwait(false))
                    {
                        ret = await db.StringIncrementAsync(cacheKey, step).ConfigureAwait(false);
                    }
                    else
                    {
                        ret = await db.StringIncrementAsync(cacheKey, valueCreator() + step).ConfigureAwait(false);

                        await SetKeyExpirationAsync(db, cacheKey, expiration);
                    }

                    return ret;
                });
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
            var db = GetDatabase(cacheKey);
            return RedisHelper.Lock(db, GetLockToken(cacheKey), Setting.LockTimeout, () =>
                {
                    long ret = 0;
                    if (db.KeyExists(cacheKey))
                    {
                        ret = db.StringDecrement(cacheKey, -step);
                    }
                    else
                    {
                        db.StringDecrement(cacheKey, valueCreator());
                        ret = db.StringDecrement(cacheKey, -step);

                        SetKeyExpiration(db, cacheKey, expiration);
                    }

                    return ret;
                });
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
            cancellationToken.ThrowIfCancellationRequested();

            cacheKey = ServiceProvider.GetCacheKey(cacheKey);
            var db = GetDatabase(cacheKey);
            return await RedisHelper.LockAsync(db, GetLockToken(cacheKey), Setting.LockTimeout, async () =>
                {
                    long ret = 0;
                    if (await db.KeyExistsAsync(cacheKey).ConfigureAwait(false))
                    {
                        ret = await db.StringDecrementAsync(cacheKey, -step).ConfigureAwait(false);
                    }
                    else
                    {
                        await db.StringDecrementAsync(cacheKey, valueCreator()).ConfigureAwait(false);
                        ret = await db.StringDecrementAsync(cacheKey, -step).ConfigureAwait(false);

                        await SetKeyExpirationAsync(db, cacheKey, expiration);
                    }

                    return ret;
                });
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

            var db = GetDatabase(cacheKey);
            return (ICacheHashSet<TKey, TValue>)_hashSet.GetOrAdd(cacheKey, () =>
                    new RedisHashSet<TKey, TValue>(Setting, cacheKey, initializeSet, db, v => Serialize(v), s => Deserialize<RedisCacheItem<TValue>>(s), checkExpiration)
                );
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
            RedisHelper.Lock(GetDatabase(token), token, timeout, action);
        }

        /// <summary>
        /// 异步的，使用事务。
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public async Task UseTransactionAsync(string token, Func<Task> func, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (RedisTransactionContext.Current != null)
            {
                await func();
                return;
            }

            using var scope = new RedisTransactionContext();
            token = ServiceProvider.GetCacheKey(token);
            await RedisHelper.LockAsync(GetDatabase(token), token, timeout, func);
        }


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

            var liveTime = Setting.SlidingTime.Add(time.Value);
            SetExpirationTime(cacheKey, () => new RelativeTime(liveTime));
        }

        /// <summary>
        /// 处理滑行时间。
        /// </summary>
        /// <param name="db"></param>
        /// <param name="cacheKey"></param>
        private async Task HandleSlidingTimeAsync(IDatabase db, string cacheKey)
        {
            if (Setting.SlidingTime == TimeSpan.Zero)
            {
                return;
            }

            var time = await db.KeyTimeToLiveAsync(cacheKey).ConfigureAwait(false);
            if (time == null || time.Value.TotalSeconds > 60)
            {
                return;
            }

            var liveTime = Setting.SlidingTime.Add(time.Value);
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
            if (!await db.KeyExistsAsync(cacheKey).ConfigureAwait(false))
            {
                return;
            }

            var expiry = GetExpirationTime(expiration);
            if (expiry != null)
            {
                await db.KeyExpireAsync(cacheKey, expiry).ConfigureAwait(false);
            }
            else
            {
                await db.KeyExpireAsync(cacheKey, (TimeSpan?)null).ConfigureAwait(false);
            }
        }

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
                var random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
                var seconds = (int)relative.Expiration.TotalSeconds;
                seconds = random.Next(seconds / 4, seconds * 2);
                return relative.Expiration.Add(TimeSpan.FromSeconds(seconds));
            }

            return null;
        }

        private string GetLockToken(string key)
        {
            return $"{key}:LOCK_TOKEN";
        }
    }
}