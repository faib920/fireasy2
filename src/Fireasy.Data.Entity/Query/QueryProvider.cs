// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Provider;
using System;
#if !NETFRAMEWORK && !NETSTANDARD2_0
using System.Collections.Generic;
#endif
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Query
{
    /// <summary>
    /// 提供对象查询的基本方法。无法继承此类。
    /// </summary>
    public sealed class QueryProvider : 
        IQueryProvider, 
        ITranslateSupport, 
        IAsyncQueryProvider, 
        IProviderAware
    {
        private readonly EntityQueryProvider entityQueryProvider;

        /// <summary>
        /// 初始化 <see cref="QueryProvider"/> 类的新实例。
        /// </summary>
        /// <param name="entityQueryProvider"></param>
        public QueryProvider(EntityQueryProvider entityQueryProvider)
        {
            this.entityQueryProvider = entityQueryProvider;
            ServiceProvider = entityQueryProvider.ServiceProvider;
            Provider = entityQueryProvider.Provider;
            ContextOptions = entityQueryProvider.ContextOptions;
        }

        /// <summary>
        /// 获取数据库提供者实例。
        /// </summary>
        public IProvider Provider { get; private set; }

        /// <summary>
        /// 获取应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// 获取参数选项。
        /// </summary>
        public EntityContextOptions ContextOptions { get; private set; }

        /// <summary>
        /// 构造一个 <see cref="IQueryable"/> 对象，该对象可计算指定表达式树所表示的查询。
        /// </summary>
        /// <param name="expression">表示 LINQ 查询的表达式树</param>
        /// <returns></returns>
        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType;
            if (expression is LambdaExpression lambda)
            {
                elementType = lambda.Type;
            }
            else
            {
                elementType = expression.Type.GetEnumerableElementType();
            }

            try
            {
                return QueryProviderCache.Create(elementType, this, expression);
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
            var executeCache = ServiceProvider.TryGetService(() => DefaultExecuteCache.Instance);
            return executeCache.TryGet(expression, GetCacheContext(), () => entityQueryProvider.Execute<TResult>(expression));
        }

#if !NETFRAMEWORK && !NETSTANDARD2_0
        public IAsyncEnumerable<TResult> ExecuteEnumerableAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var executeCache = ServiceProvider.TryGetService(() => DefaultExecuteCache.Instance);
            return executeCache.TryGet(expression, GetCacheContext(), () => entityQueryProvider.ExecuteEnumerableAsync<TResult>(expression, cancellationToken));
        }
#endif

        /// <summary>
        /// 异步的，执行指定表达式树所表示的查询。
        /// </summary>
        /// <typeparam name="TResult">执行查询所生成的值的类型。</typeparam>
        /// <param name="expression">一个表示的表达式树 LINQ 查询。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var executeCache = ServiceProvider.TryGetService(() => DefaultExecuteCache.Instance);
            return await executeCache.TryGetAsync(expression, GetCacheContext(), c => entityQueryProvider.ExecuteAsync<TResult>(expression, c), cancellationToken);
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

        private ExecuteCacheContext GetCacheContext()
        {
            return new ExecuteCacheContext(ContextOptions.CacheExecution, ContextOptions.CacheExecutionTimes);
        }
    }
}
