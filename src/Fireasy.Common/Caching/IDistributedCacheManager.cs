// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 分布式缓存管理器。
    /// </summary>
    public interface IDistributedCacheManager : ICacheManager
    {
        /// <summary>
        /// 尝试获取增量。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于初始化数据的工厂函数。</param>
        /// <param name="step">递增的步数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        long TryIncrement(string cacheKey, Func<long> factory, int step = 1, Func<ICacheItemExpiration> expiration = null);
        
        /// <summary>
        /// 异步方式尝试获取增量。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于初始化数据的工厂函数。</param>
        /// <param name="step">递增的步数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        Task<long> TryIncrementAsync(string cacheKey, Func<long> factory, int step = 1, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 尝试获取减量。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于初始化数据的工厂函数。</param>
        /// <param name="step">递减的步数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        long TryDecrement(string cacheKey, Func<long> factory, int step = 1, Func<ICacheItemExpiration> expiration = null);

        /// <summary>
        /// 异步方式尝试获取减量。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于初始化数据的工厂函数。</param>
        /// <param name="step">递减的步数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<long> TryDecrementAsync(string cacheKey, Func<long> factory, int step = 1, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default);
    }
}
