// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示对查询结果进行排序的表达式。
    /// </summary>
    public sealed class OrderExpression : DbExpression
    {
        /// <summary>
        /// 初始化 <see cref="OrderExpression"/> 类的新实例。
        /// </summary>
        /// <param name="orderType">排序的类别。</param>
        /// <param name="expression">参照的列表达式。</param>
        public OrderExpression(OrderType orderType, Expression expression)
            : base(DbExpressionType.OrderBy, expression.Type)
        {
            OrderType = orderType;
            Expression = expression;
        }

        /// <summary>
        /// 获取排序的类别。
        /// </summary>
        public OrderType OrderType { get; }

        /// <summary>
        /// 获取参照的列表达式。
        /// </summary>
        public Expression Expression { get; }
    }
}
