// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 基于内存的哈希集。无法继承此类。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class MemoryHashSet<TKey, TValue> : ICacheHashSet<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, HashCacheItem> _dictionary = new ConcurrentDictionary<TKey, HashCacheItem>();

        /// <summary>
        /// 初始化 <see cref="MemoryHashSet"/> 类的新实例。
        /// </summary>
        /// <param name="initializeSet"></param>
        public MemoryHashSet(Func<IEnumerable<Tuple<TKey, TValue, ICacheItemExpiration>>> initializeSet)
        {
            if (initializeSet != null)
            {
                initializeSet().ForEach(s => Add(s.Item1, s.Item2, s.Item3));
            }
        }

        /// <summary>
        /// 获取集合中对象的个数。
        /// </summary>
        public long Count => _dictionary.Count;

        /// <summary>
        /// 清空整个集合。
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <summary>
        /// 异步的，清空整个集合。
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// </summary>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            _dictionary.Clear();
        }
        /// <summary>
        /// 尝试从集合中获取指定 <paramref name="key"/> 的数据，如果没有则使用工厂函数添加对象到集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="valueCreator">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public TValue TryGet(TKey key, Func<TValue> valueCreator, Func<ICacheItemExpiration> expiration = null)
        {
            if (_dictionary.TryGetValue(key, out HashCacheItem cache))
            {
                if (!cache.HasExpired())
                {
                    return cache.Value;
                }
            }

            return _dictionary.GetOrAdd(key, k => new HashCacheItem(valueCreator(), expiration == null ? NeverExpired.Instance : expiration())).Value;
        }

        /// <summary>
        /// 异步的，尝试从集合中获取指定 <paramref name="key"/> 的数据，如果没有则使用工厂函数添加对象到集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="valueCreator">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public Task<TValue> TryGetAsync(TKey key, Func<Task<TValue>> valueCreator, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TryGet(key, () => valueCreator().AsSync(), expiration));
        }

        /// <summary>
        /// 将值插入到哈希集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="value"></param>
        /// <param name="expiration">判断对象过期的对象。</param>
        public void Add(TKey key, TValue value, ICacheItemExpiration expiration = null)
        {
            _dictionary.AddOrUpdate(key, k => new HashCacheItem(value, expiration), (k, v) => new HashCacheItem(value, expiration));
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
            Add(key, value, expiration);
        }

        /// <summary>
        /// 尝试从集合中获取指定 <paramref name="key"/> 的数据。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet(TKey key, out TValue value)
        {
            if (_dictionary.TryGetValue(key, out HashCacheItem cache))
            {
                if (!cache.HasExpired())
                {
                    value = cache.Value;
                    return true;
                }

                _dictionary.TryRemove(key, out _);
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
            return _dictionary.Keys;
        }

        /// <summary>
        /// 获取所有的 value。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TValue> GetValues()
        {
            foreach (var item in _dictionary.Values)
            {
                if (!item.HasExpired())
                {
                    yield return item.Value;
                }
            }
        }

        /// <summary>
        /// 异步的，获取所有的 key。
        /// </summary>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public Task<IEnumerable<TKey>> GetKeysAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetKeys());
        }

        /// <summary>
        /// 从集合中移除指定键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        public void Remove(TKey key)
        {
            _dictionary.TryRemove(key, out _);
        }

        /// <summary>
        /// 异步的，从集合中移除指定键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task RemoveAsync(TKey key, CancellationToken cancellationToken = default)
        {
            Remove(key);
        }

        /// <summary>
        /// 确定集合中是否包含指定的键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <returns></returns>
        public bool Contains(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// 异步的，确定集合中是否包含指定的键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public Task<bool> ContainsAsync(TKey key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Contains(key));
        }

        internal class HashCacheItem
        {
            public HashCacheItem()
            {
            }

            public HashCacheItem(TValue value, ICacheItemExpiration expiration)
            {
                Value = value;
                TimeSpan? expire;
                if (expiration != null && (expire = expiration.GetExpirationTime()) != null)
                {
                    ExpireTime = DateTime.Now + expire;
                }
            }

            public TValue Value { get; set; }

            public DateTime? ExpireTime { get; set; }

            public bool HasExpired()
            {
                return ExpireTime != null && ExpireTime < DateTime.Now;
            }
        }

    }
}
