// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示对列表达的参数映射。
    /// </summary>
    public sealed class ColumnAssignment
    {
        public ColumnAssignment(ColumnExpression column, Expression expression)
        {
            Column = column;
            Expression = expression;
        }

        /// <summary>
        /// 获取列表达式。
        /// </summary>
        public ColumnExpression Column { get; }

        /// <summary>
        /// 获取值表达式。
        /// </summary>
        public Expression Expression { get; }

        public override string ToString()
        {
            return $"Assign({Column}:{Expression})";
        }
    }
}
