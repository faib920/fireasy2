// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示判断是否为空的表达式。
    /// </summary>
    public sealed class IsNullExpression : DbExpression
    {
        /// <summary>
        /// 初始化 <see cref="IsNullExpression"/> 类的新实例。
        /// </summary>
        /// <param name="expression">要判断的表达式。</param>
        public IsNullExpression(Expression expression)
            : base(DbExpressionType.IsNull, typeof(bool))
        {
            Expression = expression;
        }

        /// <summary>
        /// 获取要判断的表达式。
        /// </summary>
        public Expression Expression { get; }

        /// <summary>
        /// 更新 <see cref="IsNullExpression"/> 对象。
        /// </summary>
        /// <param name="expression">要判断的表达式。</param>
        /// <returns></returns>
        public IsNullExpression Update(Expression expression)
        {
            return expression != Expression ? new IsNullExpression(expression) : this;
        }

    }
}
