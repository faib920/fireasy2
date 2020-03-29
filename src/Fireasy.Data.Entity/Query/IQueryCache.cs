// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Query
{
    /// <summary>
    /// 解析的缓存处理器。
    /// </summary>
    public interface IQueryCache
    {
        /// <summary>
        /// 尝试从缓存中获取表达式所对应的委托。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        Delegate TryGetDelegate(Expression expression, QueryCacheContext context, Func<LambdaExpression> creator);
    }

    /// <summary>
    /// 解析缓存的上下文。
    /// </summary>
    public class QueryCacheContext
    {
        public QueryCacheContext(bool? enabled, TimeSpan? times)
        {
            Enabled = enabled;
            Times = times;
        }

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
