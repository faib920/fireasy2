// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示在分组中进行的聚合运算的表达式。
    /// </summary>
    public sealed class AggregateSubqueryExpression : DbExpression
    {
        /// <summary>
        /// 初始化 <see cref="AggregateSubqueryExpression"/> 类的新实例。
        /// </summary>
        /// <param name="groupByAlias">分组表的别名。</param>
        /// <param name="aggregateInGroupSelect">聚合运算表达式。</param>
        /// <param name="aggregateAsSubquery">聚合子查询表达式。</param>
        public AggregateSubqueryExpression(TableAlias groupByAlias, Expression aggregateInGroupSelect, ScalarExpression aggregateAsSubquery)
            : base(DbExpressionType.AggregateSubquery, aggregateAsSubquery.Type)
        {
            AggregateInGroupSelect = aggregateInGroupSelect;
            GroupByAlias = groupByAlias;
            AggregateAsSubquery = aggregateAsSubquery;
        }

        /// <summary>
        /// 获取分组表的别名。
        /// </summary>
        public TableAlias GroupByAlias { get; private set; }

        /// <summary>
        /// 获取分组是的聚合运算表达式。
        /// </summary>
        public Expression AggregateInGroupSelect { get; private set; }

        /// <summary>
        /// 获取聚合子查询表达式。
        /// </summary>
        public ScalarExpression AggregateAsSubquery { get; private set; }

        /// <summary>
        /// 更新 <see cref="AggregateSubqueryExpression"/> 对象。
        /// </summary>
        /// <param name="subquery">聚合子查询表达式。</param>
        /// <returns></returns>
        public AggregateSubqueryExpression Update(ScalarExpression subquery)
        {
            if (subquery != AggregateAsSubquery)
            {
                return new AggregateSubqueryExpression(GroupByAlias, AggregateInGroupSelect, subquery);
            }
            return this;
        }

    }
}
