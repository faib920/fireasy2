// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.ObjectModel;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Removes column declarations in SelectExpression's that are not referenced
    /// </summary>
    public class UnusedColumnRemover : DbExpressionVisitor
    {
        private readonly Dictionary<TableAlias, HashSet<string>> allColumnsUsed = new Dictionary<TableAlias,HashSet<string>>();
        private bool retainAllColumns;

        public static Expression Remove(Expression expression)
        {
            return new UnusedColumnRemover().Visit(expression);
        }

        private void MarkColumnAsUsed(TableAlias alias, string name)
        {
            if (!allColumnsUsed.TryGetValue(alias, out HashSet<string> columns))
            {
                columns = new HashSet<string>();
                allColumnsUsed.Add(alias, columns);
            }

            columns.Add(name);
        }

        private bool IsColumnUsed(TableAlias alias, string name)
        {
            if (allColumnsUsed.TryGetValue(alias, out HashSet<string> columnsUsed))
            {
                if (columnsUsed != null)
                {
                    return columnsUsed.Contains(name);
                }
            }

            return false;
        }

        private void ClearColumnsUsed(TableAlias alias)
        {
            allColumnsUsed[alias] = new HashSet<string>();
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            //将列打上使用标记
            MarkColumnAsUsed(column.Alias, column.Name);
            return column;
        }

        protected override Expression VisitSubquery(SubqueryExpression subquery)
        {
            if (((DbExpressionType)subquery.NodeType == DbExpressionType.Scalar ||
                (DbExpressionType)subquery.NodeType == DbExpressionType.In) &&
                subquery.Select != null)
            {
                MarkColumnAsUsed(subquery.Select.Alias, subquery.Select.Columns[0].Name);
            }

            return base.VisitSubquery(subquery);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            // visit column projection first
            var columns = select.Columns;

            var wasRetained = retainAllColumns;
            retainAllColumns = false;

            List<ColumnDeclaration> alternate = null;
            for (int i = 0, n = select.Columns.Count; i < n; i++)
            {
                var decl = select.Columns[i];
                if (wasRetained || select.IsDistinct || IsColumnUsed(select.Alias, decl.Name))
                {
                    var expr = Visit(decl.Expression);
                    if (expr != decl.Expression)
                    {
                        decl = new ColumnDeclaration(decl.Name, expr);
                    }
                }
                else
                {
                    decl = null;  // null means it gets omitted
                }

                if (decl != select.Columns[i] && alternate == null)
                {
                    alternate = new List<ColumnDeclaration>();
                    for (int j = 0; j < i; j++)
                    {
                        alternate.Add(select.Columns[j]);
                    }
                }

                if (decl != null && alternate != null)
                {
                    alternate.Add(decl);
                }
            }

            if (alternate != null)
            {
                columns = alternate.AsReadOnly();
            }

            var take = Visit(select.Take);
            var skip = Visit(select.Skip);
            var groupbys = VisitMemberAndExpressionList(select.GroupBy);
            var orderbys = VisitOrderBy(select.OrderBy);
            var where = Visit(select.Where);
            var having = Visit(select.Having);

            var from = Visit(select.From);

            ClearColumnsUsed(select.Alias);

            //构成新的查询表达式
            select = select.Update(from, where, orderbys, groupbys, skip, take, select.Segment, select.IsDistinct, columns, having, select.IsReverse);

            retainAllColumns = wasRetained;

            return select;
        }

        protected override Expression VisitAggregate(AggregateExpression aggregate)
        {
            // COUNT(*) forces all columns to be retained in subquery
            if (aggregate.AggregateType == AggregateType.Count && aggregate.Argument == null)
            {
                retainAllColumns = true;
            }

            return base.VisitAggregate(aggregate);
        }

        protected override Expression VisitProjection(ProjectionExpression projection)
        {
            // visit mapping in reverse order
            var projector = Visit(projection.Projector);
            var select = (SelectExpression)Visit(projection.Select);
            
            return projection.Update(select, projector, projection.Aggregator);
        }

        protected override Expression VisitClientJoin(ClientJoinExpression join)
        {
            var innerKey = VisitMemberAndExpressionList(join.InnerKey);
            var outerKey = VisitMemberAndExpressionList(join.OuterKey);
            var projection = (ProjectionExpression)Visit(join.Projection);
            if (projection != join.Projection || innerKey != join.InnerKey || outerKey != join.OuterKey)
            {
                return new ClientJoinExpression(projection, outerKey, innerKey);
            }

            return join;
        }

        protected override Expression VisitJoin(JoinExpression join)
        {
            if (join.JoinType == JoinType.SingletonLeftOuter)
            {
                // first visit right side w/o looking at condition
                var right = Visit(join.Right);
                if (right is AliasedExpression ax && !allColumnsUsed.ContainsKey(ax.Alias))
                {
                    // if nothing references the alias on the right, then the join is redundant
                    return Visit(join.Left);
                }

                // otherwise do it the right way
                var cond = Visit(join.Condition);
                var left = Visit(join.Left);
                right = Visit(join.Right);
                return join.Update(join.JoinType, left, right, cond);
            }
            else
            {
                // visit join in reverse order
                var condition = Visit(join.Condition);
                var right = VisitSource(join.Right);
                var left = VisitSource(join.Left);
                return join.Update(join.JoinType, left, right, condition);
            }
        }
    }
}