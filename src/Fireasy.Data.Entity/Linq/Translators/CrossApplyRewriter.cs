// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Attempts to rewrite cross-apply and outer-apply joins as inner and left-outer joins
    /// </summary>
    public class CrossApplyRewriter : DbExpressionVisitor
    {
        public static Expression Rewrite(Expression expression)
        {
            return new CrossApplyRewriter().Visit(expression);
        }

        /// <summary>
        /// 访问 <see cref="JoinExpression"/>。
        /// </summary>
        /// <param name="join">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitJoin(JoinExpression join)
        {
            join = (JoinExpression)base.VisitJoin(join);

            if (join.JoinType == JoinType.CrossApply || join.JoinType == JoinType.OuterApply)
            {
                if (join.Right is TableExpression)
                {
                    return new JoinExpression(JoinType.CrossJoin, join.Left, join.Right, null);
                }
                else
                {
                    // Only consider rewriting cross apply if 
                    //   1) right side is a select
                    //   2) other than in the where clause in the right-side select, no left-side declared aliases are referenced
                    //   3) and has no behavior that would change semantics if the where clause is removed (like groups, aggregates, take, skip, etc).
                    // Note: it is best to attempt this after redundant subqueries have been removed.
                    if (join.Right is SelectExpression select
                        && select.Take == null
                        && select.Skip == null
                        && !AggregateChecker.HasAggregates(select)
                        && (select.GroupBy == null || select.GroupBy.Count == 0))
                    {
                        var selectWithoutWhere = select.SetWhere(null);
                        var referencedAliases = ReferencedAliasGatherer.Gather(selectWithoutWhere);
                        var declaredAliases = DeclaredAliasGatherer.Gather(join.Left);
                        referencedAliases.IntersectWith(declaredAliases);
                        if (referencedAliases.Count == 0)
                        {
                            var where = select.Where;
                            select = selectWithoutWhere;
                            var pc = ColumnProjector.ProjectColumns(CanBeColumn, where, select.Columns, select.Alias, DeclaredAliasGatherer.Gather(select.From));
                            select = select.SetColumns(pc.Columns);
                            where = pc.Projector;
                            var jt = (where == null) ? JoinType.CrossJoin : (join.JoinType == JoinType.CrossApply ? JoinType.InnerJoin : JoinType.LeftOuter);
                            return new JoinExpression(jt, join.Left, select, where);
                        }
                    }
                }
            }

            return join;
        }

        private bool CanBeColumn(Expression expr)
        {
            return expr != null && expr.NodeType == (ExpressionType)DbExpressionType.Column;
        }
    }
}
