// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示使用 JOIN 进行链接查询的表达式。
    /// </summary>
    public sealed class JoinExpression : DbExpression
    {
        /// <summary>
        /// 初始化 <see cref="JoinExpression"/> 类的新实例。
        /// </summary>
        /// <param name="joinType">JOIN 的类别。</param>
        /// <param name="left">JOIN 左边的查询表达式。</param>
        /// <param name="right">JOIN 右边的查询表达式。</param>
        /// <param name="condition">ON 条件表达式。</param>
        public JoinExpression(JoinType joinType, Expression left, Expression right, Expression condition)
            : base(DbExpressionType.Join, typeof(void))
        {
            JoinType = joinType;
            Left = left;
            Right = right;
            Condition = condition;
        }

        /// <summary>
        /// 获取 JOIN 的类别。
        /// </summary>
        public JoinType JoinType { get; }

        /// <summary>
        /// 获取 JOIN 左边的查询表达式。
        /// </summary>
        public Expression Left { get; }

        /// <summary>
        /// 获取 JOIN 右边的查询表达式。
        /// </summary>
        public Expression Right { get; }

        /// <summary>
        /// 获取 ON 条件表达式。
        /// </summary>
        public new Expression Condition { get; }

        /// <summary>
        /// 更新 <see cref="JoinExpression"/> 对象。
        /// </summary>
        /// <param name="joinType">JOIN 的类别。</param>
        /// <param name="left">JOIN 左边的查询表达式。</param>
        /// <param name="right">JOIN 右边的查询表达式。</param>
        /// <param name="condition">ON 条件表达式。</param>
        public JoinExpression Update(JoinType joinType, Expression left, Expression right, Expression condition)
        {
            if (joinType != JoinType || left != Left || right != Right || condition != Condition)
            {
                return new JoinExpression(joinType, left, right, condition);
            }
            return this;
        }

    }
}
