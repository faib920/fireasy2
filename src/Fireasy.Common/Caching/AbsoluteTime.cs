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
    /// 使用一个绝对时间作为缓存的过期检测策略，缓存项生命周期是到达该时间时为止。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class AbsoluteTime : ICacheItemExpiration
    {
        /// <summary>
        /// 使用一个绝对时间初始化 <see cref="AbsoluteTime"/> 类的新实例。
        /// </summary>
        /// <param name="absoluteTime">绝对时间。</param>
        public AbsoluteTime(DateTime absoluteTime)
        {
            ExpirationTime = absoluteTime.ToUniversalTime();
        }

        /// <summary>
        /// 使用一个时间间隔初始化 <see cref="AbsoluteTime"/> 类的新实例。
        /// </summary>
        /// <param name="timeSpan">当前系统时间后的一个时间段。</param>
        public AbsoluteTime(TimeSpan timeSpan)
            : this(DateTime.Now + timeSpan)
        {
        }

        /// <summary>
        /// 获取到期时间。
        /// </summary>
        public DateTime ExpirationTime { get; private set; }

        /// <summary>
        /// 检查缓存项是否达到过期时间。
        /// </summary>
        /// <param name="cacheItem">要检查的缓存项。</param>
        /// <returns>过期为 true，有效为 false。</returns>
        public bool HasExpired(CacheItem cacheItem)
        {
            var nowDateTime = DateTime.Now.ToUniversalTime();
            return nowDateTime.Ticks >= ExpirationTime.Ticks;
        }
    }
}
