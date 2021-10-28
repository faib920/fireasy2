using Fireasy.Common.Caching;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enyim.Caching.Memcached;
using Enyim.Caching;

namespace Fireasy.Memcached
{
    public class CacheManager : ICacheManager
    {
        private readonly IMemcachedClient _client;

        public CacheManager()
        {
            _client = new MemcachedClient();
        }

        public T Add<T>(string cacheKey, T value, TimeSpan? expire = null, CacheItemRemovedCallback removeCallback = null)
        {
            if (expire == null)
            {
                _client.Store(StoreMode.Add, cacheKey, value);
            }
            else
            {
                _client.Store(StoreMode.Add, cacheKey, value, expire.Value);
            }

            return value;
        }

        public T Add<T>(string cacheKey, T value, ICacheItemExpiration expiration, CacheItemRemovedCallback removeCallback = null)
        {
            throw new NotImplementedException();
        }

        public Task<T> AddAsync<T>(string cacheKey, T value, TimeSpan? expire = null, CacheItemRemovedCallback removeCallback = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<T> AddAsync<T>(string cacheKey, T value, ICacheItemExpiration expiration, CacheItemRemovedCallback removeCallback = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool Contains(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ContainsAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public object Get(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public TimeSpan? GetExpirationTime(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public Task<TimeSpan?> GetExpirationTimeAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ICacheHashSet<TKey, TValue> GetHashSet<TKey, TValue>(string cacheKey, Func<IEnumerable<Tuple<TKey, TValue, ICacheItemExpiration>>> initializeSet = null, bool checkExpiration = true)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetKeys(string pattern)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetKeysAsync(string pattern, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Remove(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void SetExpirationTime(string cacheKey, Func<ICacheItemExpiration> expiration)
        {
            throw new NotImplementedException();
        }

        public Task SetExpirationTimeAsync(string cacheKey, Func<ICacheItemExpiration> expiration, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public T TryGet<T>(string cacheKey, Func<T> valueCreator, Func<ICacheItemExpiration> expiration = null)
        {
            throw new NotImplementedException();
        }

        public object TryGet(Type dataType, string cacheKey, Func<object> valueCreator, Func<ICacheItemExpiration> expiration = null)
        {
            throw new NotImplementedException();
        }

        public bool TryGet<T>(string cacheKey, out T value)
        {
            throw new NotImplementedException();
        }

        public Task<T> TryGetAsync<T>(string cacheKey, Func<Task<T>> valueCreator, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<object> TryGetAsync(Type dataType, string cacheKey, Func<Task<object>> valueCreator, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
