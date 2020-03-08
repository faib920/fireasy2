// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 解析的缓存处理器。
    /// </summary>
    public interface ICacheParsingProcessor
    {
        /// <summary>
        /// 尝试从缓存中获取表达式所对应的委托。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="cacheOpt"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        Delegate TryGetDelegate(Expression expression, CacheParsingOptions cacheOpt, Func<LambdaExpression> creator);
    }

    /// <summary>
    /// 解析缓存的选项。
    /// </summary>
    public class CacheParsingOptions
    {
        public CacheParsingOptions(bool? enabled, TimeSpan? times, string cachePrefix)
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
