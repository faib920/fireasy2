// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Common.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示 IN 查询的表达式。可以使用子查询进行查询，也可以使用一个常量集合进行查询。
    /// </summary>
    public sealed class InExpression : SubqueryExpression
    {
        /// <summary>
        /// 使用子查询表达式初始化 <see cref="InExpression"/> 类的新实例。
        /// </summary>
        /// <param name="expression">IN 左边的列表达式。</param>
        /// <param name="select">子查询表达式。</param>
        public InExpression(Expression expression, SelectExpression select)
            : base(DbExpressionType.In, typeof(bool), select)
        {
            Expression = expression;
        }

        /// <summary>
        /// 使用常量表达式集合初始化 <see cref="InExpression"/> 类的新实例。
        /// </summary>
        /// <param name="expression">IN 左边的列表达式。</param>
        /// <param name="values">常量表达式集合。</param>
        public InExpression(Expression expression, IEnumerable<Expression> values)
            : base(DbExpressionType.In, typeof(bool), null)
        {
            Expression = expression;
            Values = values.ToReadOnly();
        }

        /// <summary>
        /// 获取 IN 左边的列表达式。
        /// </summary>
        public Expression Expression { get; }

        /// <summary>
        /// 表示常量的查询表达式集合。
        /// </summary>
        public ReadOnlyCollection<Expression> Values { get; }

        /// <summary>
        /// 更新 <see cref="InExpression"/> 对象。
        /// </summary>
        /// <param name="expression">IN 左边的列表达式。</param>
        /// <param name="select">子查询表达式。</param>
        /// <param name="values">常量表达式集合。</param>
        /// <returns></returns>
        public InExpression Update(Expression expression, SelectExpression select, IEnumerable<Expression> values)
        {
            if (expression != Expression || select != Select || values != Values)
            {
                return select != null ? new InExpression(expression, select) : new InExpression(expression, values);
            }
            return this;
        }

    }
}
