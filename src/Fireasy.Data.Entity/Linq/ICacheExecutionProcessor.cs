// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 执行的缓存处理器。
    /// </summary>
    public interface ICacheExecutionProcessor
    {
        /// <summary>
        /// 尝试获取数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="cacheOpt"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        T TryGet<T>(Expression expression, CacheExecutionOptions cacheOpt, Func<T> creator);

        /// <summary>
        /// 异步的，尝试获取数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="cacheOpt"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        Task<T> TryGetAsync<T>(Expression expression, CacheExecutionOptions cacheOpt, Func<CancellationToken, Task<T>> creator, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 执行缓存的选项。
    /// </summary>
    public class CacheExecutionOptions
    {
        public CacheExecutionOptions(bool? enabled, TimeSpan? times, string cachePrefix)
        {
            Enabled = enabled;
            Times = times;
            CachePrefix = cachePrefix;
        }

        /// <summary>
        /// 获取缓存的前缀。
        /// </summary>
        public string CachePrefix { get; private set; }

        /// <summary>
        /// 获取是否启用。
        /// </summary>
        public bool? Enabled { get; private set; }

        /// <summary>
        /// 获取缓存过期时间。
        /// </summary>
        public TimeSpan? Times { get; private set; }
    }
}
