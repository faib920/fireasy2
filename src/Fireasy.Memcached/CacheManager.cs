// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Fireasy.Common.Caching;

namespace Fireasy.Memcached
{
    public class CacheManager : IDistributedCacheManager
    {
        public T Add<T>(string cacheKey, T value, TimeSpan? expire = null, CacheItemRemovedCallback removeCallback = null)
        {
            throw new NotImplementedException();
        }

        public T Add<T>(string cacheKey, T value, ICacheItemExpiration expiration, CacheItemRemovedCallback removeCallback = null)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public object Get(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetKeys()
        {
            throw new NotImplementedException();
        }

        public void Remove(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public T TryGet<T>(string cacheKey, Func<T> factory, Func<ICacheItemExpiration> expiration = null)
        {
            throw new NotImplementedException();
        }

        public bool TryGet<T>(string cacheKey, out T value)
        {
            throw new NotImplementedException();
        }
    }
}
