// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using System;

namespace Fireasy.Redis
{
    internal class RedisCacheItem<TValue>
    {
        public RedisCacheItem()
        {
        }

        public RedisCacheItem(TValue value, ICacheItemExpiration expiration)
        {
            Value = value;
            var timeSpan = expiration.GetExpirationTime();
            if (timeSpan != null)
            {
                ExpireTime = DateTime.Now + timeSpan;
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
