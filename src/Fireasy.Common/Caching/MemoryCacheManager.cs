// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 基于内存的缓存管理。无法继承此类。
    /// </summary>
    public sealed class MemoryCacheManager : ICacheManager
    {
        /// <summary>
        /// 获取 <see cref="MemoryCacheManager"/> 的静态实例。
        /// </summary>
        public readonly static MemoryCacheManager Instance = new MemoryCacheManager();

        private readonly ConcurrentDictionary<string, CacheItem> cacheDictionary = new ConcurrentDictionary<string, CacheItem>();
        private readonly CacheOptimizer optimizer;

        /// <summary>
        /// 初始化 <see cref="MemoryCacheManager"/> 类的新实例。
        /// </summary>
        public MemoryCacheManager()
        {
            optimizer = new CacheOptimizer(CheckExpired);
        }

        /// <summary>
        /// 将对象插入到缓存管理器中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="value">要插入到缓存的对象。</param>
        /// <param name="expire">对象存放于缓存中的有效时间，到期后将从缓存中移除。如果此值为 null，则默认有效时间为 30 分钟。</param>
        /// <param name="removeCallback">当对象从缓存中移除时，使用该回调方法通知应用程序。</param>
        public T Add<T>(string cacheKey, T value, TimeSpan? expire = null, CacheItemRemovedCallback removeCallback = null)
        {
            var entry = CreateCacheItem(cacheKey, () => value, () => expire == null ? NeverExpired.Instance : new RelativeTime(expire.Value), removeCallback);
            cacheDictionary.AddOrUpdate(cacheKey, entry, (s, o) => entry);

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
            var entry = CreateCacheItem(cacheKey, () => value, () => expiration, removeCallback);
            cacheDictionary.AddOrUpdate(cacheKey, entry, (s, o) => entry);

            return value;
        }

        /// <summary>
        /// 确定缓存中是否包含指定的缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>如果缓存中包含指定缓存键的对象，则为 true，否则为 false。</returns>
        /// <exception cref="NotSupportedException">不支持该方法。</exception>
        public bool Contains(string cacheKey)
        {
            return cacheDictionary.ContainsKey(cacheKey);
        }

        /// <summary>
        /// 获取缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns></returns>
        public TimeSpan? GetExpirationTime(string cacheKey)
        {
            if (cacheDictionary.TryGetValue(cacheKey, out CacheItem entry))
            {
                return entry.Expiration?.GetExpirationTime();
            }

            return null;
        }

        /// <summary>
        /// 获取缓存中指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        /// <exception cref="NotSupportedException">不支持该方法。</exception>
        public object Get(string cacheKey)
        {
            if (cacheDictionary.TryGetValue(cacheKey, out CacheItem entry))
            {
                if (optimizer.Update(entry) == null)
                {
                    if (cacheDictionary.TryRemove(cacheKey, out entry))
                    {
                        NotifyCacheRemoved(entry);
                        return null;
                    }
                }

                return entry.Value;
            }

            return null;
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
            var lazy = new Lazy<CacheItem>(() => CreateCacheItem(cacheKey, factory, expiration, null));

            if (cacheDictionary.TryGetValue(cacheKey, out CacheItem entry))
            {
                //判断是否过期，移除后再添加
                if (optimizer.Update(entry) == null)
                {
                    if (cacheDictionary.TryRemove(cacheKey, out entry))
                    {
                        NotifyCacheRemoved(entry);
                    }

                    entry = cacheDictionary.GetOrAdd(cacheKey, s => lazy.Value);
                }
            }
            else
            {
                entry = cacheDictionary.GetOrAdd(cacheKey, s => lazy.Value);
            }

            return (T)entry.Value;
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
            if (cacheDictionary.TryGetValue(cacheKey, out CacheItem entry))
            {
                //判断是否过期
                if (optimizer.Update(entry) == null)
                {
                    if (cacheDictionary.TryRemove(cacheKey, out entry))
                    {
                        NotifyCacheRemoved(entry);
                        value = default(T);
                        return false;
                    }
                }

                value = (T)entry.Value;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        /// <summary>
        /// 从缓存中移除指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <exception cref="NotSupportedException">不支持该方法。</exception>
        public void Remove(string cacheKey)
        {
            if (cacheDictionary.TryRemove(cacheKey, out CacheItem entry))
            {
                NotifyCacheRemoved(entry);
            }
        }

        /// <summary>
        /// 获取所有的 key。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetKeys()
        {
            return cacheDictionary.Keys;
        }

        /// <summary>
        /// 清除所有缓存。
        /// </summary>
        public void Clear()
        {
            cacheDictionary.Clear();
        }

        /// <summary>
        /// 获取或设容量大小。
        /// </summary>
        public int Capacity { get; set; } = 1000;

        private CacheItem CreateCacheItem<T>(string cacheKey, Func<T> factory, Func<ICacheItemExpiration> expiration, CacheItemRemovedCallback removeCallback)
        {
            CheckCapacity();

            return optimizer.Update(new CacheItem(
                cacheKey,
                factory(),
                expiration == null ? null : expiration(),
                removeCallback), false);
        }

        /// <summary>
        /// 检查缓存的容量。
        /// </summary>
        private void CheckCapacity()
        {
            while (cacheDictionary.Count >= Capacity)
            {
                var genToDelete = optimizer.Discard();
                var delete = cacheDictionary.FirstOrDefault(p => p.Value.Gen == genToDelete);
                if (delete.Key != null)
                {
                    if (cacheDictionary.TryRemove(delete.Key, out CacheItem entry))
                    {
                        NotifyCacheRemoved(entry);
                    }
                }
            }
        }

        /// <summary>
        /// 通知缓存项已移除。
        /// </summary>
        /// <param name="entry"></param>
        private void NotifyCacheRemoved(CacheItem entry)
        {
            entry.NotifyRemoved?.Invoke(entry.Key, entry.Value);
        }

        /// <summary>
        /// 检查过期的缓存项。
        /// </summary>
        private void CheckExpired()
        {
            foreach (var kvp in cacheDictionary)
            {
                if (kvp.Value.HasExpired())
                {
                    if (cacheDictionary.TryRemove(kvp.Key, out CacheItem entry))
                    {
                        NotifyCacheRemoved(entry);
                    }
                }
            }
        }
    }
}
