// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Linq.Expressions;

namespace Fireasy.Common.Dynamic
{
    internal class NoThrowExpressionVisitor : Fireasy.Common.Linq.Expressions.ExpressionVisitor
    {
        internal static readonly object ErrorResult = new object();

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            // if the result of a test is to throw an error, rewrite to result an error result value
            if (node.IfFalse.NodeType == ExpressionType.Throw)
            {
                return Expression.Condition(node.Test, node.IfTrue, Expression.Constant(ErrorResult));
            }

            return base.VisitConditional(node);
        }
    }
}
