using Fireasy.Data.Entity.Linq.Translators;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 提供对特定数据源的查询进行计算的功能。
    /// </summary>
    /// <typeparam name="T">数据类型。</typeparam>
    public class QuerySet<T> : IOrderedQueryable<T>, IListSource, IQueryExportation
    {
        private Expression expression;
        private IQueryProvider provider;
        private IList<T> list;

        public QuerySet(IQueryProvider provider)
            : this(provider, null)
        {
        }

        public QuerySet(IQueryProvider provider, Type staticType)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            this.provider = provider;
            this.expression = staticType != null ? Expression.Constant(this, staticType) : Expression.Constant(this);
        }

        public QuerySet(QueryProvider provider, Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            this.provider = provider;
            this.expression = expression;
        }

        /// <summary>
        /// 获取查询解释文本。
        /// </summary>
        public string QueryText
        {
            get
            {
                var translator = provider as ITranslateSupport;
                if (translator != null)
                {
                    return translator.Translate(expression).ToString();
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
            return ExecuteList().GetEnumerator();
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
            return ExecuteList() as IList;
        }

        #endregion 实现IListSource接口

        #region 实现IQueryable接口

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public Expression Expression
        {
            get { return expression; }
        }

        public IQueryProvider Provider
        {
            get { return provider; }
        }

        #endregion 实现IQueryable接口

        private IList<T> ExecuteList()
        {
            if (list == null)
            {
                if (provider == null)
                {
                    list = new List<T>();
                }
                else
                {
                    list = ExecuteCache.TryGet(expression, () => provider.Execute<IEnumerable<T>>(expression)?.ToList());
                }
            }
            return list;
        }
    }

    internal static class QueryHelper
    {
        internal static QuerySet<T> CreateQuery<T>(IQueryProvider provider, Expression expression)
        {
            var querySet = new QuerySet<T>(provider);
            if (expression != null)
            {
                var query = (IQueryable)querySet;
                var method = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(s => s.Name == nameof(Queryable.Where));

                if (method != null)
                {
                    method = method.MakeGenericMethod(typeof(T));
                    expression = Expression.Call(method, query.Expression, expression);

                    return (QuerySet<T>)query.Provider.CreateQuery<T>(expression);
                }
            }

            return querySet;
        }
    }
}