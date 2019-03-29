// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Results;
using Fireasy.Common.Caching;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fireasy.Memcached
{
    public class CacheManager : IDistributedCacheManager
    {
        private readonly MemcachedClient cacheClient;

        public CacheManager(ILoggerFactory loggerFactory, IMemcachedClientConfiguration configuration)
        {
            cacheClient = new MemcachedClient(loggerFactory, configuration); ;
        }

        public T Add<T>(string cacheKey, T value, TimeSpan? expire = null, CacheItemRemovedCallback removeCallback = null)
        {
            var result = expire == null ? cacheClient.ExecuteStore(StoreMode.Set, cacheKey, value)
                : cacheClient.ExecuteStore(StoreMode.Set, cacheKey, value, expire.Value);

            return value;
        }

        public T Add<T>(string cacheKey, T value, ICacheItemExpiration expiration, CacheItemRemovedCallback removeCallback = null)
        {
            if (expiration is RelativeTime relative)
            {
                cacheClient.ExecuteStore(StoreMode.Set, cacheKey, value, relative.Expiration);
            }
            else if (expiration is AbsoluteTime absolute)
            {
                cacheClient.ExecuteStore(StoreMode.Set, cacheKey, value, absolute.ExpirationTime);
            }

            return value;
        }

        public void Clear()
        {
            cacheClient.FlushAll();
        }

        public bool Contains(string cacheKey)
        {
            return cacheClient.ExecuteGet(cacheKey).HasValue;
        }

        public object Get(string cacheKey)
        {
            return cacheClient.ExecuteGet(cacheKey);
        }

        public TimeSpan? GetExpirationTime(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetKeys()
        {
            throw new NotImplementedException();
        }

        public void Remove(string cacheKey)
        {
            cacheClient.ExecuteRemove(cacheKey);
        }

        public T TryGet<T>(string cacheKey, Func<T> factory, Func<ICacheItemExpiration> expiration = null)
        {
            return cacheClient.GetValueOrCreateAsync(cacheKey, 0, () => Task.FromResult(factory())).Result;
        }

        public bool TryGet<T>(string cacheKey, out T value)
        {
            IGetOperationResult result;
            if((result = cacheClient.ExecuteTryGet(cacheKey, out object cacheValue)).HasValue)
            {
                value = (T)result.Value;
                return true;
            }

            value = default(T);
            return false;
        }

        public long TryIncrement(string cacheKey, Func<long> factory, int step = 1, Func<ICacheItemExpiration> expiration = null)
        {
            throw new NotImplementedException();
        }

        public long TryDecrement(string cacheKey, Func<long> factory, int step = 1, Func<ICacheItemExpiration> expiration = null)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public object TryGet(Type dataType, string cacheKey, Func<object> factory, Func<ICacheItemExpiration> expiration = null)
        {
            throw new NotImplementedException();
        }
    }
}
