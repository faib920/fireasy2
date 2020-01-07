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

namespace Fireasy.Redis
{
    /// <summary>
    /// 基于 Redis 的哈希集。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class RedisHashSet<TKey, TValue> : ICacheHashSet<TKey, TValue>
    {
        private RedisConfigurationSetting setting;
        private string cacheKey;
        private Func<RedisCacheItem<TValue>, string> serialize;
        private Func<string, RedisCacheItem<TValue>> deserialize;

#if NETSTANDARD
        private Func<CSRedisClient> clientFunc;

        internal RedisHashSet(RedisConfigurationSetting setting, string cacheKey, Func<CSRedisClient> clientFunc, Func<RedisCacheItem<TValue>, string> serialize, Func<string, RedisCacheItem<TValue>> deserialize)
        {
            this.setting = setting;
            this.cacheKey = cacheKey;
            this.clientFunc = clientFunc;
            this.serialize = serialize;
            this.deserialize = deserialize;
        }
#else
        private Func<IDatabase> dbFunc;

        internal RedisHashSet(RedisConfigurationSetting setting, string cacheKey, Func<IDatabase> dbFunc, Func<RedisCacheItem<TValue>, string> serialize, Func<string, RedisCacheItem<TValue>> deserialize)
        {
            this.setting = setting;
            this.cacheKey = cacheKey;
            this.dbFunc = dbFunc;
            this.serialize = serialize;
            this.deserialize = deserialize;
        }
#endif

        /// <summary>
        /// 尝试从集合中获取指定 <paramref name="key"/> 的数据，如果没有则使用工厂函数添加对象到集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="factory">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public TValue TryGet(TKey key, Func<TValue> factory, Func<ICacheItemExpiration> expiration = null)
        {
            var sKey = key.ToString();
#if NETSTANDARD
            CheckCache<TValue> GetCacheValue()
            {
                if (clientFunc().HExists(cacheKey, sKey))
                {
                    var content = clientFunc().HGet(cacheKey, sKey);
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

            return RedisHelper.Lock(clientFunc(), cacheKey, TimeSpan.FromSeconds(setting.LockTimeout), () =>
                {
                    var ck1 = GetCacheValue();
                    if (ck1.HasValue)
                    {
                        return ck1.Value;
                    }

                    var value = factory();
                    var json = serialize(new RedisCacheItem<TValue>(value, expiration == null ? NeverExpired.Instance : expiration()));
                    clientFunc().HSet(cacheKey, sKey, json);
                    return value;
                });
#else
            CheckCache<TValue> GetCacheValue()
            {
                if (dbFunc().HashExists(cacheKey, sKey))
                {
                    var content = dbFunc().HashGet(cacheKey, sKey);
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

            return RedisHelper.Lock(dbFunc(), cacheKey, TimeSpan.FromSeconds(setting.LockTimeout), () =>
                {
                    var ck1 = GetCacheValue();
                    if (ck1.HasValue)
                    {
                        return ck1.Value;
                    }

                    var value = factory();
                    var json = serialize(new RedisCacheItem<TValue>(value, expiration == null ? NeverExpired.Instance : expiration()));
                    dbFunc().HashSet(cacheKey, sKey, json);
                    return value;
                });
#endif
        }

        /// <summary>
        /// 异步的，尝试从集合中获取指定 <paramref name="key"/> 的数据，如果没有则使用工厂函数添加对象到集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="factory">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<TValue> TryGetAsync(TKey key, Func<Task<TValue>> factory, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default)
        {
            var sKey = key.ToString();
#if NETSTANDARD
            async Task<CheckCache<TValue>> GetCacheValue()
            {
                if (await clientFunc().HExistsAsync(cacheKey, sKey))
                {
                    var content = await clientFunc().HGetAsync(cacheKey, sKey);
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

            return await RedisHelper.LockAsync(clientFunc(), cacheKey, TimeSpan.FromSeconds(setting.LockTimeout), async () =>
                {
                    var ck1 = await GetCacheValue();
                    if (ck1.HasValue)
                    {
                        return ck1.Value;
                    }

                    var value = await factory();
                    var json = serialize(new RedisCacheItem<TValue>(value, expiration == null ? NeverExpired.Instance : expiration()));
                    await clientFunc().HSetAsync(cacheKey, sKey, json);
                    return value;
                });
#else
            async Task<CheckCache<TValue>> GetCacheValue()
            {
                if (await dbFunc().HashExistsAsync(cacheKey, sKey))
                {
                    var content = await dbFunc().HashGetAsync(cacheKey, sKey);
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

            return await RedisHelper.LockAsync(dbFunc(), cacheKey, TimeSpan.FromSeconds(setting.LockTimeout), async () =>
                {
                    var ck1 = await GetCacheValue();
                    if (ck1.HasValue)
                    {
                        return ck1.Value;
                    }

                    var value = await factory();
                    var json = serialize(new RedisCacheItem<TValue>(value, expiration == null ? NeverExpired.Instance : expiration()));
                    await dbFunc().HashSetAsync(cacheKey, sKey, json);
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
            var json = serialize(new RedisCacheItem<TValue>(value, expiration ?? NeverExpired.Instance));
#if NETSTANDARD
            clientFunc().HSet(cacheKey, sKey, json);
#else
            dbFunc().HashSet(cacheKey, sKey, json);
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
            var json = serialize(new RedisCacheItem<TValue>(value, expiration ?? NeverExpired.Instance));
#if NETSTANDARD
            await clientFunc().HSetAsync(cacheKey, sKey, json);
#else
            await dbFunc().HashSetAsync(cacheKey, sKey, json);
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
            if (clientFunc().HExists(cacheKey, sKey))
            {
                var content = clientFunc().HGet(cacheKey, sKey);
                var item = deserialize(content);
                if (!item.HasExpired())
                {
                    value = item.Value;
                    return true;
                }

                clientFunc().HDel(cacheKey, sKey);
            }
#else
            if (dbFunc().HashExists(cacheKey, sKey))
            {
                var content = dbFunc().HashGet(cacheKey, sKey);
                var item = deserialize(content);
                if (!item.HasExpired())
                {
                    value = item.Value;
                    return true;
                }

                dbFunc().HashDelete(cacheKey, sKey);
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
            return clientFunc().HKeys(cacheKey).Select(s => s.To<TKey>());
#else
            return dbFunc().HashKeys(cacheKey).Select(s => s.To<TKey>());
#endif
        }

        /// <summary>
        /// 异步的，获取所有的 key。
        /// </summary>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<IEnumerable<TKey>> GetKeysAsync(CancellationToken cancellationToken = default)
        {
#if NETSTANDARD
            return (await clientFunc().HKeysAsync(cacheKey)).Select(s => s.To<TKey>());
#else
            return (await dbFunc().HashKeysAsync(cacheKey)).Select(s => s.To<TKey>());
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
            clientFunc().HDel(cacheKey, sKey);
#else
            dbFunc().HashDelete(cacheKey, sKey);
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
            await clientFunc().HDelAsync(cacheKey, sKey);
#else
            await dbFunc().HashDeleteAsync(cacheKey, sKey);
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
            return clientFunc().HExists(cacheKey, sKey);
#else
            return dbFunc().HashExists(cacheKey, sKey);
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
            return await clientFunc().HExistsAsync(cacheKey, sKey);
#else
            return await dbFunc().HashExistsAsync(cacheKey, sKey);
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
                return clientFunc().HLen(cacheKey);
#else
                return dbFunc().HashLength(cacheKey);
#endif
            }
        }

        /// <summary>
        /// 清空整个集合。
        /// </summary>
        public void Clear()
        {
#if NETSTANDARD
            clientFunc().Del(cacheKey);
#else
            dbFunc().KeyDelete(cacheKey);
#endif
        }
    }
}
