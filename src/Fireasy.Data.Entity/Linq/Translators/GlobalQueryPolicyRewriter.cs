// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq.Expressions;
using System.Linq.Expressions;
using Fireasy.Common.Extensions;
using System.Collections.Generic;
using System.Linq;
using Fireasy.Common.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 用于向表达式内注入全局查询策略。
    /// </summary>
    public class GlobalQueryPolicyRewriter : DbExpressionVisitor
    {
        public static Expression Rewrite(Expression expression)
        {
            return new GlobalQueryPolicyRewriter().Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            if (select.From != null && select.From.NodeType == (ExpressionType)DbExpressionType.Table)
            {
                var table = (TableExpression)select.From;
                var expressions = GlobalQueryPolicy.GetPolicies(table.Type);
                if (expressions.IsNullOrEmpty())
                {
                    return select;
                }

                var columns = select.Columns;
                var where = select.Where;

                foreach (var exp in expressions)
                {
                    where = PolicyConditionReplacer.Replace(where, exp, columns);
                }

                return select.Update(select.From, where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                        select.Segment, select.IsDistinct, select.Columns, select.Having, select.IsReverse);
            }
            else if (select.From != null)
            {
                var from = base.Visit(select.From);
                return select.Update(from, select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                        select.Segment, select.IsDistinct, select.Columns, select.Having, select.IsReverse);
            }

            return select;
        }

        private class PolicyConditionReplacer : Common.Linq.Expressions.ExpressionVisitor
        {
            private Expression where;
            private IEnumerable<ColumnDeclaration> columns;

            public static Expression Replace(Expression where, Expression expression, IEnumerable<ColumnDeclaration> columns)
            {
                var visitor = new PolicyConditionReplacer { where = where, columns = columns };
                visitor.Visit(PartialEvaluator.Eval(expression));
                return visitor.where;
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                var left = Visit(node.Left);
                var right = Visit(node.Right);

                var isUpdated = false;
                if ((DbExpressionType)left.NodeType == DbExpressionType.Column)
                {
                    isUpdated = true;
                    if (right.Type != left.Type)
                    {
                        right = Expression.Convert(right, left.Type);
                    }
                }
                else if ((DbExpressionType)right.NodeType == DbExpressionType.Column)
                {
                    isUpdated = true;
                    if (right.Type != left.Type)
                    {
                        left = Expression.Convert(left, right.Type);
                    }
                }

                if (isUpdated)
                {
                    node = node.Update(left, node.Conversion, right);
                    where = where == null ? node : Expression.And(where, node);
                }

                return node;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var property = PropertyUnity.GetProperty(node.Member.DeclaringType, node.Member.Name);
                if (property != null)
                {
                    var column = columns.FirstOrDefault(s => s.Name == node.Member.Name);
                    return column != null ? column.Expression : node;
                }

                return node;
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                var oper = Visit(node.Operand);
                if ((DbExpressionType)oper.NodeType == DbExpressionType.Column)
                {
                    return oper;
                }

                return node;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType == typeof(AnonymousMember) && node.Method.Name == "get_Item")
                {
                    var name = (node.Arguments[0] as ConstantExpression).Value.ToString();
                    var column = columns.FirstOrDefault(s => s.Name == name);
                    return column != null ? column.Expression : node;
                }

                return node;
            }
        }
    }
}
