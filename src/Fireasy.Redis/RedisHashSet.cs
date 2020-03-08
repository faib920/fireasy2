// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
#if NETSTANDARD
using CSRedis;
#else
using StackExchange.Redis;
#endif
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Fireasy.Common.Extensions;
using Fireasy.Common.Tasks;
using System.Diagnostics;
using Fireasy.Common;

namespace Fireasy.Redis
{
    /// <summary>
    /// 基于 Redis 的哈希集。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class RedisHashSet<TKey, TValue> : ICacheHashSet<TKey, TValue>
    {
        private readonly RedisConfigurationSetting setting;
        private readonly string cacheKey;
        private readonly Func<RedisCacheItem<TValue>, string> serialize;
        private readonly Func<string, RedisCacheItem<TValue>> deserialize;

#if NETSTANDARD
        private readonly CSRedisClient client;

        internal RedisHashSet(
            RedisConfigurationSetting setting, 
            string cacheKey, 
            Func<IEnumerable<Tuple<TKey, TValue, ICacheItemExpiration>>> initializeSet, 
            CSRedisClient client, 
            Func<RedisCacheItem<TValue>, string> serialize, 
            Func<string, RedisCacheItem<TValue>> deserialize)
        {
            this.setting = setting;
            this.cacheKey = cacheKey;
            this.client = client;
            this.serialize = serialize;
            this.deserialize = deserialize;

            if (initializeSet != null)
            {
                Initialize(initializeSet);
            }

            StartExpireTask();
        }
#else
        private IDatabase database;

        internal RedisHashSet(
            RedisConfigurationSetting setting,
            string cacheKey,
            Func<IEnumerable<Tuple<TKey, TValue, ICacheItemExpiration>>> initializeSet,
            IDatabase database,
            Func<RedisCacheItem<TValue>, string> serialize,
            Func<string, RedisCacheItem<TValue>> deserialize)
        {
            this.setting = setting;
            this.cacheKey = cacheKey;
            this.database = database;
            this.serialize = serialize;
            this.deserialize = deserialize;

            if (initializeSet != null)
            {
                Initialize(initializeSet);
            }

            StartExpireTask();
        }
#endif

        /// <summary>
        /// 尝试从集合中获取指定 <paramref name="key"/> 的数据，如果没有则使用函数添加对象到集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="valueCreator">用于添加缓存对象的函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public TValue TryGet(TKey key, Func<TValue> valueCreator, Func<ICacheItemExpiration> expiration = null)
        {
            var sKey = key.ToString();
#if NETSTANDARD
            CheckCache<TValue> GetCacheValue()
            {
                if (client.HExists(cacheKey, sKey))
                {
                    var content = client.HGet(cacheKey, sKey);
                    var item = deserialize(content);
                    if (!item.HasExpired())
                    {
                        return CheckCache<TValue>.Result(item.Value);
                    }
                }

                return CheckCache<TValue>.Null();
            }

            var ck = GetCacheValue();
            if (ck.HasValue)
            {
                return ck.Value;
            }

            return RedisHelper.Lock(client, string.Concat(cacheKey, ":", sKey), TimeSpan.FromSeconds(setting.LockTimeout), () =>
                {
                    var ck1 = GetCacheValue();
                    if (ck1.HasValue)
                    {
                        return ck1.Value;
                    }

                    var value = valueCreator();
                    var content = serialize(new RedisCacheItem<TValue>(value, expiration == null ? NeverExpired.Instance : expiration()));
                    client.HSet(cacheKey, sKey, content);
                    return value;
                });
#else
            CheckCache<TValue> GetCacheValue()
            {
                if (database.HashExists(cacheKey, sKey))
                {
                    var content = database.HashGet(cacheKey, sKey);
                    var item = deserialize(content);
                    if (!item.HasExpired())
                    {
                        return CheckCache<TValue>.Result(item.Value);
                    }
                }

                return CheckCache<TValue>.Null();
            }

            var ck = GetCacheValue();
            if (ck.HasValue)
            {
                return ck.Value;
            }

            return RedisHelper.Lock(database, string.Concat(cacheKey, ":", sKey), TimeSpan.FromSeconds(setting.LockTimeout), () =>
                {
                    var ck1 = GetCacheValue();
                    if (ck1.HasValue)
                    {
                        return ck1.Value;
                    }

                    var value = valueCreator();
                    var content = serialize(new RedisCacheItem<TValue>(value, expiration == null ? NeverExpired.Instance : expiration()));
                    database.HashSet(cacheKey, sKey, content);
                    return value;
                });
#endif
        }

        /// <summary>
        /// 异步的，尝试从集合中获取指定 <paramref name="key"/> 的数据，如果没有则使用函数添加对象到集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="valueCreator">用于添加缓存对象的函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<TValue> TryGetAsync(TKey key, Func<Task<TValue>> valueCreator, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default)
        {
            var sKey = key.ToString();
#if NETSTANDARD
            async Task<CheckCache<TValue>> GetCacheValue()
            {
                if (await client.HExistsAsync(cacheKey, sKey))
                {
                    var content = await client.HGetAsync(cacheKey, sKey);
                    var item = deserialize(content);
                    if (!item.HasExpired())
                    {
                        return CheckCache<TValue>.Result(item.Value);
                    }
                }

                return CheckCache<TValue>.Null();
            }

            var ck = await GetCacheValue();
            if (ck.HasValue)
            {
                return ck.Value;
            }

            return await RedisHelper.LockAsync(client, string.Concat(cacheKey, ":", sKey), TimeSpan.FromSeconds(setting.LockTimeout), async () =>
                {
                    var ck1 = await GetCacheValue();
                    if (ck1.HasValue)
                    {
                        return ck1.Value;
                    }

                    var value = await valueCreator();
                    var content = serialize(new RedisCacheItem<TValue>(value, expiration == null ? NeverExpired.Instance : expiration()));
                    await client.HSetAsync(cacheKey, sKey, content);
                    return value;
                });
#else
            async Task<CheckCache<TValue>> GetCacheValue()
            {
                if (await database.HashExistsAsync(cacheKey, sKey))
                {
                    var content = await database.HashGetAsync(cacheKey, sKey);
                    var item = deserialize(content);
                    if (!item.HasExpired())
                    {
                        return CheckCache<TValue>.Result(item.Value);
                    }
                }

                return CheckCache<TValue>.Null();
            }

            var ck = await GetCacheValue();
            if (ck.HasValue)
            {
                return ck.Value;
            }

            return await RedisHelper.LockAsync(database, string.Concat(cacheKey, ":", sKey), TimeSpan.FromSeconds(setting.LockTimeout), async () =>
                {
                    var ck1 = await GetCacheValue();
                    if (ck1.HasValue)
                    {
                        return ck1.Value;
                    }

                    var value = await valueCreator();
                    var content = serialize(new RedisCacheItem<TValue>(value, expiration == null ? NeverExpired.Instance : expiration()));
                    await database.HashSetAsync(cacheKey, sKey, content);
                    return value;
                });
#endif
        }

        /// <summary>
        /// 将值插入到哈希集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="value"></param>
        /// <param name="expiration">判断对象过期的对象。</param>
        public void Add(TKey key, TValue value, ICacheItemExpiration expiration = null)
        {
            var sKey = key.ToString();
            var content = serialize(new RedisCacheItem<TValue>(value, expiration ?? NeverExpired.Instance));
#if NETSTANDARD
            client.HSet(cacheKey, sKey, content);
#else
            database.HashSet(cacheKey, sKey, content);
#endif
        }

        /// <summary>
        /// 异步的，将值插入到哈希集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="value"></param>
        /// <param name="expiration">判断对象过期的对象。</param>
        public async Task AddAsync(TKey key, TValue value, ICacheItemExpiration expiration = null)
        {
            var sKey = key.ToString();
            var content = serialize(new RedisCacheItem<TValue>(value, expiration ?? NeverExpired.Instance));
#if NETSTANDARD
            await client.HSetAsync(cacheKey, sKey, content);
#else
            await database.HashSetAsync(cacheKey, sKey, content);
#endif
        }

        /// <summary>
        /// 尝试从集合中获取指定 <paramref name="key"/> 的数据。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet(TKey key, out TValue value)
        {
            var sKey = key.ToString();
#if NETSTANDARD
            if (client.HExists(cacheKey, sKey))
            {
                var content = client.HGet(cacheKey, sKey);
                var item = deserialize(content);
                if (!item.HasExpired())
                {
                    value = item.Value;
                    return true;
                }

                client.HDel(cacheKey, sKey);
            }
#else
            if (database.HashExists(cacheKey, sKey))
            {
                var content = database.HashGet(cacheKey, sKey);
                var item = deserialize(content);
                if (!item.HasExpired())
                {
                    value = item.Value;
                    return true;
                }

                database.HashDelete(cacheKey, sKey);
            }
#endif
            value = default;
            return false;
        }

        /// <summary>
        /// 获取所有的 key。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TKey> GetKeys()
        {
#if NETSTANDARD
            return client.HKeys(cacheKey).Select(s => s.To<TKey>());
#else
            return database.HashKeys(cacheKey).Select(s => s.To<TKey>());
#endif
        }

        /// <summary>
        /// 获取所有的 value。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TValue> GetValues()
        {
#if NETSTANDARD
            var set = client.HGetAll(cacheKey);
#else
            var set = database.HashGetAll(cacheKey);
#endif
            foreach (var kvp in set)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    var value = deserialize(kvp.Value);
                    if (value != null && !value.HasExpired())
                    {
                        yield return value.Value;
                    }
                }
            }
        }

        /// <summary>
        /// 异步的，获取所有的 key。
        /// </summary>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<IEnumerable<TKey>> GetKeysAsync(CancellationToken cancellationToken = default)
        {
#if NETSTANDARD
            return (await client.HKeysAsync(cacheKey)).Select(s => s.To<TKey>());
#else
            return (await database.HashKeysAsync(cacheKey)).Select(s => s.To<TKey>());
#endif
        }

        /// <summary>
        /// 从集合中移除指定键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        public void Remove(TKey key)
        {
            var sKey = key.ToString();
#if NETSTANDARD
            client.HDel(cacheKey, sKey);
#else
            database.HashDelete(cacheKey, sKey);
#endif
        }

        /// <summary>
        /// 异步的，从集合中移除指定键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task RemoveAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var sKey = key.ToString();
#if NETSTANDARD
            await client.HDelAsync(cacheKey, sKey);
#else
            await database.HashDeleteAsync(cacheKey, sKey);
#endif
        }

        /// <summary>
        /// 确定集合中是否包含指定的键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <returns></returns>
        public bool Contains(TKey key)
        {
            var sKey = key.ToString();
#if NETSTANDARD
            return client.HExists(cacheKey, sKey);
#else
            return database.HashExists(cacheKey, sKey);
#endif
        }

        /// <summary>
        /// 异步的，确定集合中是否包含指定的键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <returns></returns>
        public async Task<bool> ContainsAsync(TKey key)
        {
            var sKey = key.ToString();
#if NETSTANDARD
            return await client.HExistsAsync(cacheKey, sKey);
#else
            return await database.HashExistsAsync(cacheKey, sKey);
#endif
        }

        /// <summary>
        /// 获取集合中对象的个数。
        /// </summary>
        public long Count
        {
            get
            {
#if NETSTANDARD
                return client.HLen(cacheKey);
#else
                return database.HashLength(cacheKey);
#endif
            }
        }

        /// <summary>
        /// 清空整个集合。
        /// </summary>
        public void Clear()
        {
#if NETSTANDARD
            client.Del(cacheKey);
#else
            database.KeyDelete(cacheKey);
#endif
        }

        private void Initialize(Func<IEnumerable<Tuple<TKey, TValue, ICacheItemExpiration>>> initializeSet)
        {
            try
            {
                var initValues = initializeSet();
                if (initValues == null)
                {
                    return;
                }

#if NETSTANDARD
                var array = initValues.ToArray();
                var keyValues = new object[array.Length * 2];
                for (var i = 0; i < array.Length; i++)
                {
                    var content = serialize(new RedisCacheItem<TValue>(array[i].Item2, array[i].Item3 ?? NeverExpired.Instance));
                    keyValues[i * 2] = array[i].Item1.ToString();
                    keyValues[i * 2 + 1] = content;
                }

                client.HMSet(cacheKey, keyValues);
#else
                var array = initValues.ToArray();
                var entries = new HashEntry[array.Length];
                for (var i = 0; i < array.Length; i++)
                {
                    var content = serialize(new RedisCacheItem<TValue>(array[i].Item2, array[i].Item3 ?? NeverExpired.Instance));
                    entries[i] = new HashEntry(array[i].Item1.ToString(), content);
                }

                database.HashSet(cacheKey, entries);
#endif
            }
            catch
            { }
        }

        private void StartExpireTask()
        {
            var scheduler = TaskSchedulerFactory.CreateScheduler();
            if (scheduler != null)
            {
                var startOption = new StartOptions<ExpireTaskExecutor>(TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1))
                {
                    Initializer = s =>
                    {
                        s.CacheKey = cacheKey;
                        s.Deserialize = deserialize;
#if NETSTANDARD
                        s.Client = client;
#else
                        s.Database = database;
#endif
                    }
                };

                scheduler.StartAsync(startOption);
            }
        }

        private class ExpireTaskExecutor : IAsyncTaskExecutor
        {
            public string CacheKey { get; set; }

#if NETSTANDARD
            public CSRedisClient Client { get; set; }
#else
            public IDatabase Database { get; set; }
#endif

            public Func<string, RedisCacheItem<TValue>> Deserialize { get; set; }

            public async Task ExecuteAsync(TaskExecuteContext context, CancellationToken cancellationToken = default)
            {
#if NETSTANDARD
                var set = Client.HGetAll(CacheKey);
#else
                var set = Database.HashGetAll(CacheKey);
#endif
                set.ForEachParallel(async s =>
                {
                    if (!string.IsNullOrEmpty(s.Value))
                    {
                        await RedisHelper.LockAsync(
#if NETSTANDARD
                            Client, string.Concat(CacheKey, ":", s.Key), 
#else
                            Database, string.Concat(CacheKey, ":", s.Name),
#endif
                            TimeSpan.FromSeconds(10), async () =>
                            {
                                try
                                {
                                    var value = Deserialize(s.Value);
                                    if (value != null && value.HasExpired())
                                    {
#if NETSTANDARD
                                        await Client.HDelAsync(CacheKey, s.Key);
#else
                                        await Database.HashDeleteAsync(CacheKey, s.Name);
#endif
                                    }
                                }
                                catch(Exception exp)
                                {
                                    Tracer.Error($"RedisHashSet ExpireTaskExecutor Execute Error:\n{exp.Output()}");
                                }
                            });
                    }
                });
            }
        }
    }
}
