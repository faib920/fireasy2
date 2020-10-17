// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Common.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示 Entity 查询的表达式。
    /// </summary>
    public class SelectExpression : AliasedExpression
    {
        public SelectExpression(
            TableAlias alias,
            IEnumerable<ColumnDeclaration> columns,
            Expression from,
            Expression where,
            IEnumerable<OrderExpression> orderBy,
            IEnumerable<Expression> groupBy,
            bool isDistinct,
            Expression skip,
            Expression take,
            Expression segment,
            Expression having,
            bool reverse
            )
            : base(DbExpressionType.Select, typeof(void), alias)
        {
            Columns = columns.ToReadOnly();
            IsDistinct = isDistinct;
            From = from;
            Where = where;
            OrderBy = orderBy.ToReadOnly();
            GroupBy = groupBy.ToReadOnly();
            Take = take;
            Skip = skip;
            Segment = segment;
            Having = having;
            IsReverse = reverse;
        }

        public SelectExpression(
            TableAlias alias,
            IEnumerable<ColumnDeclaration> columns,
            Expression from,
            Expression where,
            IEnumerable<OrderExpression> orderBy,
            IEnumerable<Expression> groupBy,
            Expression having
            )
            : this(alias, columns, from, where, orderBy, groupBy, false, null, null, null, having, false)
        {
        }

        public SelectExpression(
            TableAlias alias, IEnumerable<ColumnDeclaration> columns,
            Expression from, Expression where
            )
            : this(alias, columns, from, where, null, null, null)
        {
        }

        /// <summary>
        /// 获取返回的列表达式集合。
        /// </summary>
        public ReadOnlyCollection<ColumnDeclaration> Columns { get; }

        /// <summary>
        /// 获取查询的表或子查询表达式。
        /// </summary>
        public Expression From { get; }

        /// <summary>
        /// 获取条件表达式。
        /// </summary>
        public Expression Where { get; }

        /// <summary>
        /// 获取排序表达式集合。
        /// </summary>
        public ReadOnlyCollection<OrderExpression> OrderBy { get; }

        /// <summary>
        /// 获取分组表达式集合。
        /// </summary>
        public ReadOnlyCollection<Expression> GroupBy { get; }

        /// <summary>
        /// 获取 Having 表达式。
        /// </summary>
        public Expression Having { get; }

        /// <summary>
        /// 获取是否使用 Distinct 关键字。
        /// </summary>
        public bool IsDistinct { get; }

        /// <summary>
        /// 获取跳过的数量表达式。
        /// </summary>
        public Expression Skip { get; }

        /// <summary>
        /// 获取返回的数量表达式。
        /// </summary>
        public Expression Take { get; }

        /// <summary>
        /// 获取分段表达式。
        /// </summary>
        public Expression Segment { get; }

        public bool IsReverse { get; }

        /// <summary>
        /// 更新 <see cref="SelectExpression"/> 对象。
        /// </summary>
        /// <param name="from">查询的表或子查询表达式。</param>
        /// <param name="where">条件表达式。</param>
        /// <param name="orderBy">排序表达式集合。</param>
        /// <param name="groupBy">分组表达式集合。</param>
        /// <param name="skip">跳过的数量表达式。</param>
        /// <param name="take">返回的数量表达式。</param>
        /// <param name="segment">分段表达式。</param>
        /// <param name="isDistinct">是否使用 Distinct 关键字。</param>
        /// <param name="columns">返回的列表达式集合。</param>
        /// <returns></returns>
        public SelectExpression Update(
            Expression from, Expression where,
            IEnumerable<OrderExpression> orderBy, IEnumerable<Expression> groupBy,
            Expression skip, Expression take,
            Expression segment,
            bool isDistinct,
            IEnumerable<ColumnDeclaration> columns,
            Expression having,
            bool reverse
            )
        {
            if (from != From
                || where != Where
                || orderBy != OrderBy
                || groupBy != GroupBy
                || take != Take
                || skip != Skip
                || isDistinct != IsDistinct
                || columns != Columns
                || segment != Segment
                || reverse != IsReverse
                || having != Having
                )
            {
                return new SelectExpression(Alias, columns, from, where, orderBy, groupBy, isDistinct, skip, take, segment, having, reverse);
            }
            return this;
        }

        public SelectExpression Reverse(bool isReverse)
        {
            if (IsReverse != isReverse)
            {
                return new SelectExpression(Alias, Columns, From, Where, OrderBy, GroupBy, IsDistinct, Skip, Take, Segment, Having, isReverse);
            }

            return this;
        }
    }
}
