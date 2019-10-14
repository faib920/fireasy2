// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 定义支持执行异步查询的 <see cref="IQueryable"/> 对象。
    /// </summary>
    public interface IAsyncQueryProvider : IQueryProvider
    {
#if !NETFRAMEWORK && !NETSTANDARD2_0
        IAsyncEnumerable<TResult> ExecuteEnumerableAsync<TResult>(Expression expression, CancellationToken cancellationToken);
#endif

        /// <summary>
        /// 异步的，执行指定的表达式树所表示的强类型查询。
        /// </summary>
        /// <typeparam name="TResult">执行查询所生成的值的类型。</typeparam>
        /// <param name="expression">一个表示的表达式树 LINQ 查询。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken);
    }
}
