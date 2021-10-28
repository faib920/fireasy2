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
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#if NETSTANDARD2_1_OR_GREATER
using System.Threading;
#endif

namespace Fireasy.Data.Entity.Query
{
    /// <summary>
    /// 提供对特定数据源的查询进行计算的功能。
    /// </summary>
    /// <typeparam name="T">数据类型。</typeparam>
    public class QuerySet<T> : IOrderedQueryable<T>, IListSource, IQueryExportation, IContextTypeAware, IServiceProviderAccessor
#if NETSTANDARD2_1_OR_GREATER
        , IAsyncEnumerable<T>
#endif
    {
        private IList _list;

        protected QuerySet()
        {
        }

        /// <summary>
        /// 初始化 <see cref="QuerySet"/> 类的新实例。
        /// </summary>
        /// <param name="provider"></param>
        public QuerySet(IQueryProvider provider)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            var instance = new QuerySet<T> { Expression = Expression.Constant(null, typeof(T)), Provider = provider };
            Expression = Expression.Constant(instance, typeof(QuerySet<T>));
        }

        /// <summary>
        /// 初始化 <see cref="QuerySet"/> 类的新实例。
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="expression"></param>
        public QuerySet(IQueryProvider provider, Expression expression)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        /// <summary>
        /// 获取 <see cref="EntityContext"/> 的类型。
        /// </summary>
        public Type ContextType
        {
            get
            {
                if (Provider is IContextTypeAware cta)
                {
                    return cta.ContextType;
                }

                return null;
            }
        }

        /// <summary>
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider
        {
            get
            {
                if (Provider is IServiceProviderAccessor spa)
                {
                    return spa.ServiceProvider;
                }

                return null;
            }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// 获取查询解释文本。
        /// </summary>
        public string QueryText
        {
            get
            {
                if (Provider is ITranslateSupport translator)
                {
                    return translator.Translate(Expression).ToString();
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 返回枚举器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            var enumerable = Provider.Execute<IEnumerable<T>>(Expression);
            return enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region 实现IListSource接口

        bool IListSource.ContainsListCollection
        {
            get { return false; }
        }

        IList IListSource.GetList()
        {
            return _list ?? (_list = Provider.Execute<IEnumerable<T>>(Expression).ToList());
        }

        #endregion 实现IListSource接口

        #region 实现IQueryable接口

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public Expression Expression { get; internal set; }

        public IQueryProvider Provider { get; private set; }

        #endregion 实现IQueryable接口
#if NETSTANDARD2_1_OR_GREATER
        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return ((IAsyncQueryProvider)Provider).ExecuteEnumerableAsync<T>(Expression, cancellationToken).GetAsyncEnumerator();
        }
#endif
    }

    internal static class QueryHelper
    {
        private class MethodCache
        {
            internal protected static readonly MethodInfo Where = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Queryable.Where));
        }

        internal static QuerySet<T> CreateQuery<T>(IQueryProvider provider, Expression expression)
        {
            var querySet = new QuerySet<T>(provider);
            if (expression != null)
            {
                expression = Expression.Call(MethodCache.Where.MakeGenericMethod(typeof(T)), querySet.Expression, expression);
                querySet.Expression = expression;
            }

            return querySet;
        }
    }
}