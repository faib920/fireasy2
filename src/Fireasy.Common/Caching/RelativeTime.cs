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
    /// 使用一个相对时间作为缓存的过期检测策略，缓存项生命周期是自放入管理器之时起到指定的时间间隔为止，但是每一次访问缓存项，其过期时间将后延。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class RelativeTime : ICacheItemExpiration, IExpirationTime
    {
        /// <summary>
        /// 最后一次访问的时间。
        /// </summary>
        private DateTime lastAccessTime;

        public static RelativeTime Default = new RelativeTime(TimeSpan.FromMinutes(30));

        /// <summary>
        /// 初始化 <see cref="RelativeTime"/> 类的新实例。
        /// </summary>
        /// <param name="timeSpan">一个时间间隔。</param>
        public RelativeTime(TimeSpan timeSpan)
        {
            Expiration = timeSpan;
            lastAccessTime = DateTime.Now;
        }

        public static implicit operator RelativeTime(TimeSpan value)
        {
            return new RelativeTime(value);
        }

        /// <summary>
        /// 获取时间间隔。
        /// </summary>
        public TimeSpan Expiration { get; private set; }

        /// <summary>
        /// 检查缓存项是否达到过期时间。
        /// </summary>
        /// <returns>过期为 true，有效为 false。</returns>
        public bool HasExpired()
        {
            var isExpired = lastAccessTime.Add(Expiration) <= DateTime.Now;
            if (!isExpired)
            {
                lastAccessTime = DateTime.Now;
            }

            return isExpired;
        }

        /// <summary>
        /// 获取到期时间。
        /// </summary>
        /// <returns></returns>
        public TimeSpan? GetExpirationTime()
        {
            return lastAccessTime.Add(Expiration) - DateTime.Now;
        }
    }
}
