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
    /// 提供缓存项过期检测的方法。
    /// </summary>
    public interface ICacheItemExpiration
    {
        /// <summary>
        /// 返回缓存项是否过期。
        /// </summary>
        /// <returns>过期为 true，否则为 false。</returns>
        bool HasExpired();

        /// <summary>
        /// 获取到期时间。
        /// </summary>
        /// <returns></returns>
        TimeSpan? GetExpirationTime();
    }

    public interface IExpirationTime
    {
        TimeSpan Expiration { get; }
    }
}
