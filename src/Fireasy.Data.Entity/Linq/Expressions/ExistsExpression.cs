// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)


namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示判断是否存在的表达式。
    /// </summary>
    public sealed class ExistsExpression : SubqueryExpression
    {
        /// <summary>
        /// 初始化 <see cref="ExistsExpression"/> 类的新实例。
        /// </summary>
        /// <param name="select">要判断的查询表达式。</param>
        public ExistsExpression(SelectExpression select)
            : base(DbExpressionType.Exists, typeof(bool), select)
        {
        }

        /// <summary>
        /// 更新 <see cref="ExistsExpression"/> 对象。
        /// </summary>
        /// <param name="select">要判断的查询表达式。</param>
        /// <returns></returns>
        public ExistsExpression Update(SelectExpression select)
        {
            return @select != Select ? new ExistsExpression(@select) : this;
        }

        public override string ToString()
        {
            return $"Exists({Select})";
        }
    }
}
