// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;
using System.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Removes joins expressions that are identical to joins that already exist
    /// </summary>
    public class RedundantJoinRemover : DbExpressionVisitor
    {
        private Dictionary<TableAlias, TableAlias> map = new Dictionary<TableAlias,TableAlias>();

        public static Expression Remove(Expression expression)
        {
            return new RedundantJoinRemover().Visit(expression);
        }

        protected override Expression VisitJoin(JoinExpression join)
        {
            var result = base.VisitJoin(join);
            join = result as JoinExpression;

            if (join != null)
            {
                var right = join.Right as AliasedExpression;
                if (right != null)
                {
                    var similarRight = (AliasedExpression)FindSimilarRight(join.Left as JoinExpression, join);
                    if (similarRight != null)
                    {
                        map.Add(right.Alias, similarRight.Alias);
                        return join.Left;
                    }
                }
            }

            return result;
        }

        private Expression FindSimilarRight(JoinExpression join, JoinExpression compareTo)
        {
            if (join == null)
            {
                return null;
            }

            if (join.JoinType == compareTo.JoinType)
            {
                if (join.Right.NodeType == compareTo.Right.NodeType
                    && DbExpressionComparer.AreEqual(join.Right, compareTo.Right))
                {
                    if (join.Condition == compareTo.Condition)
                    {
                        return join.Right;
                    }

                    var scope = new ScopedDictionary<TableAlias, TableAlias>(null);
                    scope.Add(((AliasedExpression)join.Right).Alias, ((AliasedExpression)compareTo.Right).Alias);

                    if (DbExpressionComparer.AreEqual(null, scope, join.Condition, compareTo.Condition))
                    {
                        return join.Right;
                    }
                }
            }

            return FindSimilarRight(join.Left as JoinExpression, compareTo) ??
                FindSimilarRight(join.Right as JoinExpression, compareTo);
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            return map.TryGetValue(column.Alias, out TableAlias mapped) ?
                new ColumnExpression(column.Type, mapped, column.Name, column.MapInfo) : column;
        }
    }
}
