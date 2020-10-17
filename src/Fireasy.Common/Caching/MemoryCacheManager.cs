// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using Fireasy.Common.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 基于内存的缓存管理。无法继承此类。
    /// </summary>
    public sealed class MemoryCacheManager : IMemoryCacheManager
    {
        /// <summary>
        /// 获取 <see cref="MemoryCacheManager"/> 的静态实例。
        /// </summary>
        public readonly static MemoryCacheManager Instance = new MemoryCacheManager();

        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCacheStrategy _cacheStrategy;
        private MemoryStorage _storage;

        /// <summary>
        /// 初始化 <see cref="MemoryCacheManager"/> 类的新实例。
        /// </summary>
        public MemoryCacheManager()
            : this(ContainerUnity.GetContainer())
        {
        }

        /// <summary>
        /// 初始化 <see cref="MemoryCacheManager"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        public MemoryCacheManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _cacheStrategy = _serviceProvider.TryGetService<IMemoryCacheStrategy>();
        }

        private MemoryStorage MemoryStorage
        {
            get
            {
                return SingletonLocker.Lock(ref _storage, () =>
                {
                    var storage = _cacheStrategy != null && _cacheStrategy.UseStandaloneStorage ? new MemoryStorage() : MemoryStorage.Default;
                    storage.AddCacheRemovedEventHandler(NotifyCacheRemoved);
                    return storage;
                });
            }
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
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            var func = new Func<string, CacheItem>(key => CreateCacheItem(key, () => value, () => expire == null ? RelativeTime.Default : new RelativeTime(expire.Value), removeCallback));
            MemoryStorage.CacheDictionary.AddOrUpdate(cacheKey, k => func(k), (k, v) => func(k));

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
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            var func = new Func<string, CacheItem>(key => CreateCacheItem(key, () => value, () => expiration ?? RelativeTime.Default, removeCallback));
            MemoryStorage.CacheDictionary.AddOrUpdate(cacheKey, k => func(k), (k, v) => func(k));

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
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            return MemoryStorage.CacheDictionary.ContainsKey(cacheKey);
        }

        /// <summary>
        /// 获取缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns></returns>
        public TimeSpan? GetExpirationTime(string cacheKey)
        {
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            if (MemoryStorage.CacheDictionary.TryGetValue(cacheKey, out CacheItem entry))
            {
                return entry.Expiration?.GetExpirationTime();
            }

            return null;
        }

        /// <summary>
        /// 设置缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        public void SetExpirationTime(string cacheKey, Func<ICacheItemExpiration> expiration)
        {
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            if (MemoryStorage.CacheDictionary.TryGetValue(cacheKey, out CacheItem entry))
            {
                entry.Expiration = expiration == null ? NeverExpired.Instance : expiration();
            }
        }

        /// <summary>
        /// 获取缓存中指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        /// <exception cref="NotSupportedException">不支持该方法。</exception>
        public object Get(string cacheKey)
        {
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            if (MemoryStorage.CacheDictionary.TryGetValue(cacheKey, out CacheItem entry))
            {
                if (MemoryStorage.UpdateEntry(entry) == null)
                {
                    if (MemoryStorage.CacheDictionary.TryRemove(cacheKey, out entry))
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
        /// 获取缓存中指定缓存键的对象。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        public T Get<T>(string cacheKey)
        {
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            var value = Get(cacheKey);
            if (value != null && typeof(T).IsAssignableFrom(value.GetType()))
            {
                return (T)value;
            }

            return default;
        }

        /// <summary>
        /// 尝试获取指定缓存键的对象，如果没有则使用工厂函数添加对象到缓存中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="valueCreator">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public T TryGet<T>(string cacheKey, Func<T> valueCreator, Func<ICacheItemExpiration> expiration = null)
        {
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            if (MemoryStorage.CacheDictionary.TryGetValue(cacheKey, out CacheItem entry))
            {
                //判断是否过期，移除后再添加
                if (entry != null && entry.Value != null && MemoryStorage.UpdateEntry(entry) == null)
                {
                    if (MemoryStorage.CacheDictionary.TryRemove(cacheKey, out entry))
                    {
                        NotifyCacheRemoved(entry);
                    }

                    entry = MemoryStorage.CacheDictionary.GetOrAdd(cacheKey, k => CreateCacheItem(cacheKey, valueCreator, expiration, null));
                }
            }
            else
            {
                entry = MemoryStorage.CacheDictionary.GetOrAdd(cacheKey, k => CreateCacheItem(cacheKey, valueCreator, expiration, null));
            }

            return (T)entry.Value;
        }

        /// <summary>
        /// 尝试获取指定缓存键的对象，如果没有则使用工厂函数添加对象到缓存中。
        /// </summary>
        /// <param name="dataType">数据类型。</param>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="valueCreator">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public object TryGet(Type dataType, string cacheKey, Func<object> valueCreator, Func<ICacheItemExpiration> expiration = null)
        {
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            if (MemoryStorage.CacheDictionary.TryGetValue(cacheKey, out CacheItem entry))
            {
                //判断是否过期，移除后再添加
                if (entry != null && entry.Value != null && MemoryStorage.UpdateEntry(entry) == null)
                {
                    if (MemoryStorage.CacheDictionary.TryRemove(cacheKey, out entry))
                    {
                        NotifyCacheRemoved(entry);
                    }

                    entry = MemoryStorage.CacheDictionary.GetOrAdd(cacheKey, k => CreateCacheItem(cacheKey, valueCreator, expiration, null));
                }
            }
            else
            {
                entry = MemoryStorage.CacheDictionary.GetOrAdd(cacheKey, k => CreateCacheItem(cacheKey, valueCreator, expiration, null));
            }

            return entry.Value;
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
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            if (MemoryStorage.CacheDictionary.TryGetValue(cacheKey, out CacheItem entry))
            {
                //判断是否过期
                if (entry != null && entry.Value != null && MemoryStorage.UpdateEntry(entry) == null)
                {
                    if (MemoryStorage.CacheDictionary.TryRemove(cacheKey, out entry))
                    {
                        NotifyCacheRemoved(entry);
                        value = default;
                        return false;
                    }
                }

                value = (T)entry.Value;
                return true;
            }
            else
            {
                value = default;
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
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            if (MemoryStorage.CacheDictionary.TryRemove(cacheKey, out CacheItem entry))
            {
                NotifyCacheRemoved(entry);
            }

            if (MemoryStorage.HashSet.ContainsKey(cacheKey))
            {
                MemoryStorage.HashSet.TryRemove(cacheKey, out object _);
            }
        }

        /// <summary>
        /// 获取所有的 key。
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public IEnumerable<string> GetKeys(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return MemoryStorage.CacheDictionary.Keys;
            }

            return MemoryStorage.CacheDictionary.Keys.Where(s => Regex.IsMatch(s, pattern));
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
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            return (ICacheHashSet<TKey, TValue>)MemoryStorage.HashSet.GetOrAdd(cacheKey, k => new MemoryHashSet<TKey, TValue>(initializeSet));
        }

        /// <summary>
        /// 清除所有缓存。
        /// </summary>
        public void Clear()
        {
            MemoryStorage.CacheDictionary.Clear();
            MemoryStorage.HashSet.Clear();
        }

        /// <summary>
        /// 获取或设容量大小。默认为 1000。
        /// </summary>
        public int Capacity { get; set; } = 1000;

        private CacheItem CreateCacheItem<T>(string cacheKey, Func<T> valueCreator, Func<ICacheItemExpiration> expiration, CacheItemRemovedCallback removeCallback)
        {
            MemoryStorage.CheckCapacity(cacheKey, Capacity);

            return new CacheItem(
                cacheKey,
                valueCreator(),
                expiration == null ? null : expiration(),
                removeCallback);
        }

        private async Task<CacheItem> CreateCacheItemAsync<T>(string cacheKey, Func<Task<T>> valueCreator, Func<ICacheItemExpiration> expiration, CacheItemRemovedCallback removeCallback)
        {
            MemoryStorage.CheckCapacity(cacheKey, Capacity);

            return new CacheItem(
                cacheKey,
                await valueCreator(),
                expiration == null ? null : expiration(),
                removeCallback);
        }

        /// <summary>
        /// 通知缓存项已移除。
        /// </summary>
        /// <param name="entry"></param>
        private void NotifyCacheRemoved(CacheItem entry)
        {
            entry.NotifyRemoved?.Invoke(entry.Key, entry.Value);
        }

        async Task<T> ICacheManager.AddAsync<T>(string cacheKey, T value, TimeSpan? expire, CacheItemRemovedCallback removeCallback, CancellationToken cancellationToken)
        {
            return Add(cacheKey, value, expire, removeCallback);
        }

        async Task<T> ICacheManager.AddAsync<T>(string cacheKey, T value, ICacheItemExpiration expiration, CacheItemRemovedCallback removeCallback, CancellationToken cancellationToken)
        {
            return Add(cacheKey, value, expiration, removeCallback);
        }

        async Task<bool> ICacheManager.ContainsAsync(string cacheKey, CancellationToken cancellationToken)
        {
            return Contains(cacheKey);
        }

        async Task<object> ICacheManager.GetAsync(string cacheKey, CancellationToken cancellationToken)
        {
            return Task.FromResult(Get(cacheKey));
        }

        async Task<T> ICacheManager.GetAsync<T>(string cacheKey, CancellationToken cancellationToken)
        {
            return Get<T>(cacheKey);
        }

        async Task<TimeSpan?> ICacheManager.GetExpirationTimeAsync(string cacheKey, CancellationToken cancellationToken)
        {
            return GetExpirationTime(cacheKey);
        }

        async Task ICacheManager.SetExpirationTimeAsync(string cacheKey, Func<ICacheItemExpiration> expiration, CancellationToken cancellationToken)
        {
            SetExpirationTime(cacheKey, expiration);
        }

        async Task<T> ICacheManager.TryGetAsync<T>(string cacheKey, Func<Task<T>> valueCreator, Func<ICacheItemExpiration> expiration, CancellationToken cancellationToken)
        {
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            if (MemoryStorage.CacheDictionary.TryGetValue(cacheKey, out CacheItem entry))
            {
                //判断是否过期，移除后再添加
                if (entry != null && entry.Value != null && MemoryStorage.UpdateEntry(entry) == null)
                {
                    if (MemoryStorage.CacheDictionary.TryRemove(cacheKey, out entry))
                    {
                        NotifyCacheRemoved(entry);
                    }

                    entry = await MemoryStorage.CacheDictionary.GetOrAddAsync(cacheKey, k => CreateCacheItemAsync(k, valueCreator, expiration, null));
                }
            }
            else
            {
                entry = await MemoryStorage.CacheDictionary.GetOrAddAsync(cacheKey, k => CreateCacheItemAsync(k, valueCreator, expiration, null));
            }

            return (T)entry.Value;
        }

        async Task<object> ICacheManager.TryGetAsync(Type dataType, string cacheKey, Func<Task<object>> valueCreator, Func<ICacheItemExpiration> expiration, CancellationToken cancellationToken)
        {
            cacheKey = _serviceProvider.GetCacheKey(cacheKey);
            if (MemoryStorage.CacheDictionary.TryGetValue(cacheKey, out CacheItem entry))
            {
                //判断是否过期，移除后再添加
                if (entry != null && entry.Value != null && MemoryStorage.UpdateEntry(entry) == null)
                {
                    if (MemoryStorage.CacheDictionary.TryRemove(cacheKey, out entry))
                    {
                        NotifyCacheRemoved(entry);
                    }

                    entry = await MemoryStorage.CacheDictionary.GetOrAddAsync(cacheKey, k => CreateCacheItemAsync(k, valueCreator, expiration, null));
                }
            }
            else
            {
                entry = await MemoryStorage.CacheDictionary.GetOrAddAsync(cacheKey, k => CreateCacheItemAsync(k, valueCreator, expiration, null));
            }

            return entry.Value;
        }

        async Task<IEnumerable<string>> ICacheManager.GetKeysAsync(string pattern, CancellationToken cancellationToken)
        {
            return GetKeys(pattern);
        }

        async Task ICacheManager.RemoveAsync(string cacheKey, CancellationToken cancellationToken)
        {
            Remove(cacheKey);
        }

        async Task ICacheManager.ClearAsync(CancellationToken cancellationToken)
        {
            Clear();
        }
    }
}
