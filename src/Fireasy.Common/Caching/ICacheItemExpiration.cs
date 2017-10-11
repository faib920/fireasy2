// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

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
        /// <param name="cacheItem">要检查的缓存项。</param>
        /// <returns>过期为 true，否则为 false。</returns>
        bool HasExpired(CacheItem cacheItem);
    }
}
