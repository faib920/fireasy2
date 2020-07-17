// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Attempt to rewrite cross joins as inner joins
    /// </summary>
    public class CrossJoinRewriter : DbExpressionVisitor
    {
        private Expression _currentWhere;

        public static Expression Rewrite(Expression expression)
        {
            return new CrossJoinRewriter().Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            var saveWhere = _currentWhere;
            try
            {
                _currentWhere = select.Where;
                var result = (SelectExpression)base.VisitSelect(select);
                if (_currentWhere != result.Where)
                {
                    return result.SetWhere(_currentWhere);
                }

                return result;
            }
            finally
            {
                _currentWhere = saveWhere;
            }
        }

        protected override Expression VisitJoin(JoinExpression join)
        {
            join = (JoinExpression)base.VisitJoin(join);
            if (join.JoinType == JoinType.CrossJoin && _currentWhere != null)
            {
                // try to figure out which parts of the current where expression can be used for a join condition
                var declaredLeft = DeclaredAliasGatherer.Gather(join.Left);
                var declaredRight = DeclaredAliasGatherer.Gather(join.Right);
                var declared = new HashSet<TableAlias>(declaredLeft.Union(declaredRight));
                var exprs = _currentWhere.Split(ExpressionType.And, ExpressionType.AndAlso);
                var good = exprs.Where(e => CanBeJoinCondition(e, declaredLeft, declaredRight, declared)).ToList();
                if (good.Count > 0)
                {
                    var condition = good.Join(ExpressionType.And);
                    join = join.Update(JoinType.InnerJoin, join.Left, join.Right, condition);
                    var newWhere = exprs.Where(e => !good.Contains(e)).Join(ExpressionType.And);
                    _currentWhere = newWhere;
                }
            }
            return join;
        }

        private bool CanBeJoinCondition(Expression expression, HashSet<TableAlias> left, HashSet<TableAlias> right, HashSet<TableAlias> all)
        {
            // an expression is good if it has at least one reference to an alias from both left & right sets and does
            // not have any additional references that are not in both left & right sets
            var referenced = ReferencedAliasGatherer.Gather(expression);
            var leftOkay = referenced.Intersect(left).Any();
            var rightOkay = referenced.Intersect(right).Any();
            var subset = referenced.IsSubsetOf(all);
            return leftOkay && rightOkay && subset;
        }

    }
}
