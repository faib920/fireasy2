// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示一个期间段的查询的表达式。
    /// </summary>
    public sealed class BetweenExpression : DbExpression
    {
        /// <summary>
        /// 初始化 <see cref="BetweenExpression"/> 类的新实例。
        /// </summary>
        /// <param name="argument">参数表达式。</param>
        /// <param name="lower">条件中小的一边的表达式。</param>
        /// <param name="upper">条件中大的一边的表达式。</param>
        public BetweenExpression(Expression argument, Expression lower, Expression upper)
            : base(DbExpressionType.Between, argument.Type)
        {
            Argument = argument;
            Lower = lower;
            Upper = upper;
        }

        /// <summary>
        /// 获取参数表达式。
        /// </summary>
        public Expression Argument { get; }

        /// <summary>
        /// 获取条件中小的一边的表达式。
        /// </summary>
        public Expression Lower { get; }

        /// <summary>
        /// 获取条件中大的一边的表达式。
        /// </summary>
        public Expression Upper { get; }

        /// <summary>
        /// 更新 <see cref="BetweenExpression"/> 对象。
        /// </summary>
        /// <param name="argument">参数表达式。</param>
        /// <param name="lower">条件中小的一边的表达式。</param>
        /// <param name="upper">条件中大的一边的表达式。</param>
        /// <returns></returns>
        public BetweenExpression Update(Expression argument, Expression lower, Expression upper)
        {
            if (argument != Argument ||
                lower != Lower ||
                upper != Upper)
            {
                return new BetweenExpression(argument, lower, upper);
            }
            return this;
        }

    }
}
