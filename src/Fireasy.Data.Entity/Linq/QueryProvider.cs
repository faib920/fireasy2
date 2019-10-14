// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq.Translators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 提供对象查询的基本方法。无法继承此类。
    /// </summary>
    public sealed class QueryProvider : IQueryProvider, ITranslateSupport, IAsyncQueryProvider
    {
        private readonly IEntityQueryProvider entityQueryProvider;

        /// <summary>
        /// 初始化 <see cref="QueryProvider"/> 类的新实例。
        /// </summary>
        /// <param name="provider"></param>
        public QueryProvider(IEntityQueryProvider provider)
        {
            entityQueryProvider = provider;
        }

        /// <summary>
        /// 构造一个 <see cref="IQueryable"/> 对象，该对象可计算指定表达式树所表示的查询。
        /// </summary>
        /// <param name="expression">表示 LINQ 查询的表达式树</param>
        /// <returns></returns>
        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = null;
            var lambda = expression as LambdaExpression;
            if (lambda != null)
            {
                elementType = lambda.Type;
            }
            else
            {
                elementType = expression.Type.GetEnumerableElementType();
            }

            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(QuerySet<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        /// <summary>
        /// 构造一个 <see cref="IQueryable"/> 对象，该对象可计算指定表达式树所表示的查询。
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="expression">表示 LINQ 查询的表达式树</param>
        /// <returns></returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new QuerySet<TElement>(this, expression);
        }

        /// <summary>
        /// 执行指定表达式树所表示的查询。
        /// </summary>
        /// <param name="expression">表示 LINQ 查询的表达式树</param>
        /// <returns></returns>
        public object Execute(Expression expression)
        {
            ExecuteCache.TryExpire(expression);
            return entityQueryProvider.Execute(expression);
        }

        /// <summary>
        /// 执行指定表达式树所表示的查询。
        /// </summary>
        /// <typeparam name="TResult">执行查询所生成的值的类型。</typeparam>
        /// <param name="expression">表示 LINQ 查询的表达式树</param>
        /// <returns></returns>
        public TResult Execute<TResult>(Expression expression)
        {
            if (!ExecuteCache.CanCache(expression))
            {
                return (TResult)entityQueryProvider.Execute(expression);
            }

            return ExecuteCache.TryGet(expression, () => (TResult)entityQueryProvider.Execute(expression));
        }

#if !NETFRAMEWORK && !NETSTANDARD2_0
        public IAsyncEnumerable<TResult> ExecuteEnumerableAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            if (!ExecuteCache.CanCache(expression))
            {
                return entityQueryProvider.ExecuteEnumerableAsync<TResult>(expression, cancellationToken);
            }
            else
            {
                return ExecuteCache.TryGet(expression, () => entityQueryProvider.ExecuteEnumerableAsync<TResult>(expression, cancellationToken));
            }
        }
#endif

        /// <summary>
        /// 异步的，执行指定表达式树所表示的查询。
        /// </summary>
        /// <typeparam name="TResult">执行查询所生成的值的类型。</typeparam>
        /// <param name="expression">一个表示的表达式树 LINQ 查询。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            if (!ExecuteCache.CanCache(expression))
            {
                return await entityQueryProvider.ExecuteAsync<TResult>(expression);
            }

            return await ExecuteCache.TryGet(expression, () => entityQueryProvider.ExecuteAsync<TResult>(expression, cancellationToken));
        }

        /// <summary>
        /// 执行表达式的翻译。
        /// </summary>
        /// <param name="expression">表示 LINQ 查询的表达式树。</param>
        /// <param name="option">指定解析的选项。</param>
        /// <returns>翻译结果。</returns>
        TranslateResult ITranslateSupport.Translate(Expression expression, TranslateOptions option)
        {
            return entityQueryProvider.Translate(expression, option);
        }
    }
}
