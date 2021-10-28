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

namespace Fireasy.Data.Entity.Query
{
    /// <summary>
    /// 执行的缓存处理器。
    /// </summary>
    public interface IExecuteCache
    {
        /// <summary>
        /// 尝试获取数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        T TryGet<T>(Expression expression, ExecuteCacheContext context, Func<T> creator);

        /// <summary>
        /// 异步的，尝试获取数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        Task<T> TryGetAsync<T>(Expression expression, ExecuteCacheContext context, Func<CancellationToken, Task<T>> creator, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 用于评估表达式能否使用缓存。
    /// </summary>
    public interface IExecuteCacheEvaluator
    {
        /// <summary>
        /// 评估表达式能够被缓存。
        /// </summary>
        /// <param name="resultType"></param>
        /// <param name="elementType"></param>
        /// <returns></returns>
        bool Evaluate(ResultType resultType, Type elementType);
    }

    public enum ResultType
    {
        Enumerable,
        Single
    }

    /// <summary>
    /// 执行缓存的上下文。
    /// </summary>
    public class ExecuteCacheContext
    {
        public ExecuteCacheContext(bool? enabled, TimeSpan? times)
        {
            Enabled = enabled;
            Times = times;
        }

        /// <summary>
        /// 获取是否启用。
        /// </summary>
        public bool? Enabled { get; }

        /// <summary>
        /// 获取缓存过期时间。
        /// </summary>
        public TimeSpan? Times { get; }
    }
}
