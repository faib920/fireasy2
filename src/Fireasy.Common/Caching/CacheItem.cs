// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 放入缓存中的数据的扩展结构。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class CacheItem
    {
        /// <summary>
        /// 获取项的键名。
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// 获取项的代数。当被客户端访问时，代数递增。
        /// </summary>
        public short Gen { get; internal set; }

        /// <summary>
        /// 获取数据的值。
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// 获取或设置数据从缓存中移除时的回调方法。
        /// </summary>
        public CacheItemRemovedCallback NotifyRemoved { get; set; }

        /// <summary>
        /// 获取对象过期检测对象。
        /// </summary>
        public ICacheItemExpiration Expiration { get; internal set; }

        /// <summary>
        /// 初始化 <see cref="CacheItem"/> 类的新实例。
        /// </summary>
        /// <param name="key">键名。</param>
        /// <param name="value">要缓存的数据的值。</param>
        /// <param name="expiration">数据存放的有效时间。</param>
        /// <param name="notifyRemoved">缓存移除通知。</param>
        public CacheItem(string key, object value, ICacheItemExpiration expiration, CacheItemRemovedCallback notifyRemoved)
        {
            //默认为30分钟的有效期
            Expiration = expiration ?? RelativeTime.Default;
            if (expiration is NeverExpired)
            {
                Gen = -1;
            }

            Key = key;
            Value = value;
            NotifyRemoved = notifyRemoved;
        }

        /// <summary>
        /// 检查是否到期。
        /// </summary>
        /// <returns>已经超过到期时间，则为 true，否则为 false。</returns>
        public bool HasExpired()
        {
            return Expiration != null ? Expiration.HasExpired() : false;
        }
    }
}
