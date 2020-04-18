// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using CSRedis;
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
        private readonly Func<IEnumerable<Tuple<TKey, TValue, ICacheItemExpiration>>> initializeSet;

        private readonly CSRedisClient client;

        internal RedisHashSet(
            RedisConfigurationSetting setting,
            string cacheKey,
            Func<IEnumerable<Tuple<TKey, TValue, ICacheItemExpiration>>> initializeSet,
            CSRedisClient client,
            Func<RedisCacheItem<TValue>, string> serialize,
            Func<string, RedisCacheItem<TValue>> deserialize,
            bool checkExpiration)
        {
            this.setting = setting;
            this.cacheKey = cacheKey;
            this.client = client;
            this.serialize = serialize;
            this.deserialize = deserialize;
            this.initializeSet = initializeSet;

            Initialize();

            if (checkExpiration)
            {
                StartExpireTask();
            }
        }

        /// <summary>
        /// 尝试从集合中获取指定 <paramref name="key"/> 的数据，如果没有则使用函数添加对象到集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="valueCreator">用于添加缓存对象的函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public TValue TryGet(TKey key, Func<TValue> valueCreator, Func<ICacheItemExpiration> expiration = null)
        {
            TryReInitialize();

            var sKey = key.ToString();
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

            return RedisHelper.Lock(client, string.Concat(cacheKey, ":", sKey), setting.LockTimeout, () =>
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
            TryReInitialize();

            var sKey = key.ToString();
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

            return await RedisHelper.LockAsync(client, string.Concat(cacheKey, ":", sKey), setting.LockTimeout, async () =>
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
            client.HSet(cacheKey, sKey, content);
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
            await client.HSetAsync(cacheKey, sKey, content);
        }

        /// <summary>
        /// 尝试从集合中获取指定 <paramref name="key"/> 的数据。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet(TKey key, out TValue value)
        {
            TryReInitialize();

            var sKey = key.ToString();
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

            value = default;
            return false;
        }

        /// <summary>
        /// 获取所有的 key。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TKey> GetKeys()
        {
            TryReInitialize();

            return client.HKeys(cacheKey).Select(s => s.To<TKey>()).ToArray();
        }

        /// <summary>
        /// 异步的，获取所有的 key。
        /// </summary>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<IEnumerable<TKey>> GetKeysAsync(CancellationToken cancellationToken = default)
        {
            TryReInitialize();

            return (await client.HKeysAsync(cacheKey)).Select(s => s.To<TKey>()).ToArray();
        }

        /// <summary>
        /// 获取所有的 value。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TValue> GetValues()
        {
            TryReInitialize();

            var set = client.HGetAll(cacheKey);
            var values = new List<TValue>();
            foreach (var kvp in set)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    var value = deserialize(kvp.Value);
                    if (value != null && !value.HasExpired())
                    {
                        values.Add(value.Value);
                    }
                }
            }

            return values;
        }

        /// <summary>
        /// 从集合中移除指定键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        public void Remove(TKey key)
        {
            var sKey = key.ToString();
            client.HDel(cacheKey, sKey);
        }

        /// <summary>
        /// 异步的，从集合中移除指定键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task RemoveAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var sKey = key.ToString();
            await client.HDelAsync(cacheKey, sKey);
        }

        /// <summary>
        /// 确定集合中是否包含指定的键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <returns></returns>
        public bool Contains(TKey key)
        {
            TryReInitialize();

            var sKey = key.ToString();
            return client.HExists(cacheKey, sKey);
        }

        /// <summary>
        /// 异步的，确定集合中是否包含指定的键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <returns></returns>
        public async Task<bool> ContainsAsync(TKey key)
        {
            TryReInitialize();

            var sKey = key.ToString();
            return await client.HExistsAsync(cacheKey, sKey);
        }

        /// <summary>
        /// 获取集合中对象的个数。
        /// </summary>
        public long Count
        {
            get
            {
                TryReInitialize();

                return client.HLen(cacheKey);
            }
        }

        /// <summary>
        /// 清空整个集合。
        /// </summary>
        public void Clear()
        {
            client.Del(cacheKey);
        }

        private void Initialize()
        {
            if (initializeSet == null)
            {
                return;
            }

            var initValues = initializeSet();
            if (initValues == null)
            {
                return;
            }

            var array = initValues.ToArray();
            var keyValues = new object[array.Length * 2];
            for (var i = 0; i < array.Length; i++)
            {
                var content = serialize(new RedisCacheItem<TValue>(array[i].Item2, array[i].Item3 ?? NeverExpired.Instance));
                keyValues[i * 2] = array[i].Item1.ToString();
                keyValues[i * 2 + 1] = content;
            }

            client.HMSet(cacheKey, keyValues);
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
                        s.Client = client;
                    }
                };

                scheduler.StartExecutor(startOption);
            }
        }

        /// <summary>
        /// 尝试重新初始化。
        /// </summary>
        private void TryReInitialize()
        {
            var exists = client.Exists(cacheKey);
            if (!exists)
            {
                Initialize();
            }
        }

        private class ExpireTaskExecutor : ITaskExecutor
        {
            public string CacheKey { get; set; }

            public CSRedisClient Client { get; set; }

            public Func<string, RedisCacheItem<TValue>> Deserialize { get; set; }

            public void Execute(TaskExecuteContext context)
            {
                Tracer.Debug($"RedisHashSet clear is executing for '{CacheKey}'.");

                var set = Client.HGetAll(CacheKey);
                set.ForEach(s =>
                {
                    if (!string.IsNullOrEmpty(s.Value))
                    {
                        RedisHelper.Lock(
                            Client, string.Concat(CacheKey, ":", s.Key),
                            TimeSpan.FromSeconds(10), () =>
                            {
                                try
                                {
                                    var value = Deserialize(s.Value);
                                    if (value != null && value.HasExpired())
                                    {
                                        Client.HDel(CacheKey, s.Key);
                                    }
                                }
                                catch (Exception exp)
                                {
                                    Tracer.Error($"RedisHashSet ExpireTaskExecutor throw exception:\n{exp.Output()}");
                                }
                            });
                    }
                });
            }
        }
    }
}
