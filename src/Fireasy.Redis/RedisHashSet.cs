// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using CSRedis;
using Fireasy.Common;
using Fireasy.Common.Caching;
using Fireasy.Common.Extensions;
using Fireasy.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Redis
{
    /// <summary>
    /// 基于 Redis 的哈希集。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class RedisHashSet<TKey, TValue> : ICacheHashSet<TKey, TValue>
    {
        private readonly RedisConfigurationSetting _setting;
        private readonly string _cacheKey;
        private readonly Func<RedisCacheItem<TValue>, string> _serialize;
        private readonly Func<string, RedisCacheItem<TValue>> _deserialize;
        private readonly Func<IEnumerable<Tuple<TKey, TValue, ICacheItemExpiration>>> _initializeSet;

        private readonly CSRedisClient _client;

        internal RedisHashSet(
            RedisConfigurationSetting setting,
            string cacheKey,
            Func<IEnumerable<Tuple<TKey, TValue, ICacheItemExpiration>>> initializeSet,
            CSRedisClient client,
            Func<RedisCacheItem<TValue>, string> serialize,
            Func<string, RedisCacheItem<TValue>> deserialize,
            bool checkExpiration)
        {
            _setting = setting;
            _cacheKey = cacheKey;
            _client = client;
            _serialize = serialize;
            _deserialize = deserialize;
            _initializeSet = initializeSet;

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
            var content = _client.HGet(_cacheKey, sKey);
            if (!string.IsNullOrEmpty(content))
            {
                var item = _deserialize(content);
                if (!item.HasExpired())
                {
                    return item.Value;
                }
            }

            var value = valueCreator();
            content = _serialize(new RedisCacheItem<TValue>(value, expiration == null ? NeverExpired.Instance : expiration()));
            _client.HSet(_cacheKey, sKey, content);
            return value;
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
            cancellationToken.ThrowIfCancellationRequested();

            TryReInitialize();

            var sKey = key.ToString();
            var content = await _client.HGetAsync(_cacheKey, sKey);
            if (!string.IsNullOrEmpty(content))
            {
                var item = _deserialize(content);
                if (!item.HasExpired())
                {
                    return item.Value;
                }
            }

            var value = await valueCreator();
            content = _serialize(new RedisCacheItem<TValue>(value, expiration == null ? NeverExpired.Instance : expiration()));
            await _client.HSetAsync(_cacheKey, sKey, content);
            return value;
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
            var content = _serialize(new RedisCacheItem<TValue>(value, expiration ?? NeverExpired.Instance));
            _client.HSet(_cacheKey, sKey, content);
        }

        /// <summary>
        /// 异步的，将值插入到哈希集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="value"></param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task AddAsync(TKey key, TValue value, ICacheItemExpiration expiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sKey = key.ToString();
            var content = _serialize(new RedisCacheItem<TValue>(value, expiration ?? NeverExpired.Instance));
            await _client.HSetAsync(_cacheKey, sKey, content);
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
            if (_client.HExists(_cacheKey, sKey))
            {
                var content = _client.HGet(_cacheKey, sKey);
                var item = _deserialize(content);
                if (!item.HasExpired())
                {
                    value = item.Value;
                    return true;
                }

                _client.HDel(_cacheKey, sKey);
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

            return _client.HKeys(_cacheKey).Select(s => s.To<TKey>()).ToArray();
        }

        /// <summary>
        /// 异步的，获取所有的 key。
        /// </summary>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<IEnumerable<TKey>> GetKeysAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TryReInitialize();

            return (await _client.HKeysAsync(_cacheKey)).Select(s => s.To<TKey>()).ToArray();
        }

        /// <summary>
        /// 获取所有的 value。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TValue> GetValues()
        {
            TryReInitialize();

            var set = _client.HGetAll(_cacheKey);
            var values = new List<TValue>();
            foreach (var kvp in set)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    var value = _deserialize(kvp.Value);
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
            _client.HDel(_cacheKey, sKey);
        }

        /// <summary>
        /// 异步的，从集合中移除指定键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task RemoveAsync(TKey key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sKey = key.ToString();
            await _client.HDelAsync(_cacheKey, sKey);
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
            return _client.HExists(_cacheKey, sKey);
        }

        /// <summary>
        /// 异步的，确定集合中是否包含指定的键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<bool> ContainsAsync(TKey key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TryReInitialize();

            var sKey = key.ToString();
            return await _client.HExistsAsync(_cacheKey, sKey);
        }

        /// <summary>
        /// 获取集合中对象的个数。
        /// </summary>
        public long Count
        {
            get
            {
                TryReInitialize();

                return _client.HLen(_cacheKey);
            }
        }

        /// <summary>
        /// 清空整个集合。
        /// </summary>
        public void Clear()
        {
            _client.Del(_cacheKey);
        }

        /// <summary>
        /// 异步的，清空整个集合。
        /// </summary>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _client.DelAsync(_cacheKey);
        }

        private void Initialize()
        {
            if (_initializeSet == null)
            {
                return;
            }

            var initValues = _initializeSet();
            if (initValues == null)
            {
                return;
            }

            var array = initValues.ToArray();
            var keyValues = new object[array.Length * 2];
            for (var i = 0; i < array.Length; i++)
            {
                var content = _serialize(new RedisCacheItem<TValue>(array[i].Item2, array[i].Item3 ?? NeverExpired.Instance));
                keyValues[i * 2] = array[i].Item1.ToString();
                keyValues[i * 2 + 1] = content;
            }

            _client.HMSet(_cacheKey, keyValues);
        }

        private void StartExpireTask()
        {
            var scheduler = TaskSchedulerFactory.CreateScheduler();
            if (scheduler != null)
            {
                var r = new Random(DateTime.Now.Millisecond);
                var startOption = new StartOptions<ExpireTaskExecutor>(TimeSpan.FromSeconds(r.Next(1, 30)), TimeSpan.FromSeconds(r.Next(60, 130)))
                {
                    Initializer = s =>
                    {
                        s.CacheKey = _cacheKey;
                        s.Deserialize = _deserialize;
                        s.Client = _client;
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
            var exists = _client.Exists(_cacheKey);
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
                        try
                        {
                            var value = Deserialize(s.Value);
                            if (value != null && value.HasExpired())
                            {
                                if (Client.HExists(CacheKey, s.Key))
                                {
                                    Client.HDel(CacheKey, s.Key);
                                }
                            }
                        }
                        catch (Exception exp)
                        {
                            Tracer.Error($"RedisHashSet ExpireTaskExecutor throw exception:\n{exp.Output()}");
                        }
                    }
                });
            }
        }
    }
}
