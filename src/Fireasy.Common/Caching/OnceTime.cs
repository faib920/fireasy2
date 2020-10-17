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
    /// 表示缓存项的有效期只有一次，首次访问后将过期。无法继承此类。
    /// </summary>
    public sealed class OnceTime : ICacheItemExpiration
    {
        /// <summary>
        /// 记录访问次数的变量。
        /// </summary>
        private int _times = 0;

        /// <summary>
        /// 检查缓存项是否达到过期时间。
        /// </summary>
        /// <returns>过期为 true，有效为 false。</returns>
        public bool HasExpired()
        {
            return _times++ >= 1;//OK
        }

        /// <summary>
        /// 获取到期时间。
        /// </summary>
        /// <returns></returns>
        public TimeSpan? GetExpirationTime()
        {
            return _times < 1 ? TimeSpan.MaxValue : (TimeSpan?)null;
        }
    }
}
