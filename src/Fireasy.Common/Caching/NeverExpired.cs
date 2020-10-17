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
    /// 表示缓存项永不过期。无法继承此类。
    /// </summary>
    public sealed class NeverExpired : ICacheItemExpiration
    {
        /// <summary>
        /// 获取 <see cref="NeverExpired"/> 的静态实例。
        /// </summary>
        public readonly static ICacheItemExpiration Instance = new NeverExpired();

        /// <summary>
        /// 检查缓存项是否达到过期时间。
        /// </summary>
        /// <returns>始终为 false。</returns>
        public bool HasExpired()
        {
            return false;
        }

        /// <summary>
        /// 获取到期时间。
        /// </summary>
        /// <returns></returns>
        public TimeSpan? GetExpirationTime()
        {
            return null;
        }
    }
}
