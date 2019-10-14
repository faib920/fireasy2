// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Fireasy.Data.Entity.Linq.Translators;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 为实体提供 LINQ 查询的支持。
    /// </summary>
    public interface IEntityQueryProvider : ITranslateSupport
    {
        /// <summary>
        /// 执行 <see cref="Expression"/> 的查询，返回查询结果。
        /// </summary>
        /// <param name="expression">表示 LINQ 查询的表达式树。</param>
        /// <returns></returns>
        object Execute(Expression expression);

        /// <summary>
        /// 异步的，执行 <see cref="Expression"/> 的查询，返回查询结果。
        /// </summary>
        /// <param name="expression">表示 LINQ 查询的表达式树。</param>
        /// <returns></returns>
        Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default);

#if !NETFRAMEWORK && !NETSTANDARD2_0
        IAsyncEnumerable<TResult> ExecuteEnumerableAsync<TResult>(Expression expression, CancellationToken cancellationToken);
#endif
    }
}
