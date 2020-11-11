// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Removes select expressions that don't add any additional semantic value
    /// </summary>
    public class RedundantSubqueryRemover : DbExpressionVisitor
    {
        public static Expression Remove(Expression expression)
        {
            expression = new RedundantSubqueryRemover().Visit(expression);
            expression = SubqueryMerger.Merge(expression);
            return expression;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelect(select);

            // first remove all purely redundant subqueries
            var redundant = RedundantSubqueryGatherer.Gather(select.From);
            if (redundant != null)
            {
                select = SubqueryRemover.Remove(select, redundant);
            }

            return select;
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            proj = (ProjectionExpression)base.VisitProjection(proj);
            if (proj.Select.From is SelectExpression)
            {
                var redundant = RedundantSubqueryGatherer.Gather(proj.Select);
                if (redundant != null)
                {
                    proj = SubqueryRemover.Remove(proj, redundant);
                }
            }

            return proj;
        }

        internal static bool IsSimpleProjection(SelectExpression select)
        {
            foreach (ColumnDeclaration decl in select.Columns)
            {
                if (!(decl.Expression is ColumnExpression col) || decl.Name != col.Name)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool IsNameMapProjection(SelectExpression select)
        {
            if (select.From is TableExpression)
            {
                return false;
            }

            if (!(select.From is SelectExpression fromSelect) || select.Columns.Count != fromSelect.Columns.Count)
            {
                return false;
            }

            var fromColumns = fromSelect.Columns;
            // test that all columns in 'select' are refering to columns in the same position
            // in from.
            for (int i = 0, n = select.Columns.Count; i < n; i++)
            {
                if (!(select.Columns[i].Expression is ColumnExpression col) || !(col.Name == fromColumns[i].Name))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool IsInitialProjection(SelectExpression select)
        {
            return select.From is TableExpression;
        }

        private class RedundantSubqueryGatherer : DbExpressionVisitor
        {
            private List<SelectExpression> _redundant;

            private RedundantSubqueryGatherer()
            {
            }

            internal static List<SelectExpression> Gather(Expression source)
            {
                var gatherer = new RedundantSubqueryGatherer();
                gatherer.Visit(source);
                return gatherer._redundant;
            }

            private static bool IsRedudantSubquery(SelectExpression select)
            {
                return (IsSimpleProjection(select) || IsNameMapProjection(select))
                    && !select.IsDistinct
                    && !select.IsReverse
                    && select.Take == null
                    && select.Skip == null
                    && select.Where == null
                    && select.Segment == null
                    && (select.OrderBy == null || select.OrderBy.Count == 0)
                    && (select.GroupBy == null || select.GroupBy.Count == 0);
            }

            protected override Expression VisitSelect(SelectExpression select)
            {
                if (IsRedudantSubquery(select))
                {
                    if (_redundant == null)
                    {
                        _redundant = new List<SelectExpression>();
                    }

                    _redundant.Add(select);
                }

                return select;
            }

            protected override Expression VisitSubquery(SubqueryExpression subquery)
            {
                // don't gather inside scalar & exists
                return subquery;
            }
        }

        private class SubqueryMerger : DbExpressionVisitor
        {
            private bool _isTopLevel = true;

            private SubqueryMerger()
            {
            }

            internal static Expression Merge(Expression expression)
            {
                return new SubqueryMerger().Visit(expression);
            }

            protected override Expression VisitSelect(SelectExpression select)
            {
                var wasTopLevel = _isTopLevel;
                _isTopLevel = false;

                select = (SelectExpression)base.VisitSelect(select);

                // next attempt to merge subqueries that would have been removed by the above
                // logic except for the existence of a where clause
                while (CanMergeWithFrom(select, wasTopLevel))
                {
                    var fromSelect = GetLeftMostSelect(select.From);

                    // remove the redundant subquery
                    select = SubqueryRemover.Remove(select, fromSelect);

                    // merge where expressions 
                    var where = select.Where;
                    if (fromSelect.Where != null)
                    {
                        if (where != null)
                        {
                            where = fromSelect.Where.And(where);
                        }
                        else
                        {
                            where = fromSelect.Where;
                        }
                    }

                    var orderBy = select.OrderBy != null && select.OrderBy.Count > 0 ? select.OrderBy : fromSelect.OrderBy;
                    var groupBy = select.GroupBy != null && select.GroupBy.Count > 0 ? select.GroupBy : fromSelect.GroupBy;
                    var skip = select.Skip ?? fromSelect.Skip;
                    var take = select.Take ?? fromSelect.Take;
                    var segment = select.Segment ?? fromSelect.Segment;
                    bool isDistinct = select.IsDistinct | fromSelect.IsDistinct;

                    if (where != select.Where
                        || orderBy != select.OrderBy
                        || groupBy != select.GroupBy
                        || isDistinct != select.IsDistinct
                        || skip != select.Skip
                        || take != select.Take)
                    {
                        select = new SelectExpression(select.Alias, select.Columns, select.From, where, orderBy, groupBy, isDistinct, skip, take, segment, select.Having, select.IsReverse);
                    }
                }

                return select;
            }

            private static SelectExpression GetLeftMostSelect(Expression source)
            {
                if (source is SelectExpression select)
                {
                    return select;
                }

                if (source is JoinExpression join)
                {
                    return GetLeftMostSelect(join.Left);
                }

                return null;
            }

            private static bool IsColumnProjection(SelectExpression select)
            {
                for (int i = 0, n = select.Columns.Count; i < n; i++)
                {
                    var cd = select.Columns[i];
                    if (cd.Expression.NodeType != (ExpressionType)DbExpressionType.Column &&
                        cd.Expression.NodeType != ExpressionType.Constant &&
                        cd.Expression.NodeType != (ExpressionType)DbExpressionType.AggregateContact)
                    {
                        return false;
                    }
                }

                return true;
            }

            private static bool CanMergeWithFrom(SelectExpression select, bool isTopLevel)
            {
                var fromSelect = GetLeftMostSelect(select.From);
                if (fromSelect == null)
                {
                    return false;
                }

                if (!IsColumnProjection(fromSelect))
                {
                    return false;
                }

                var selHasNameMapProjection = IsNameMapProjection(select);
                var selHasOrderBy = select.OrderBy != null && select.OrderBy.Count > 0;
                var selHasGroupBy = select.GroupBy != null && select.GroupBy.Count > 0;
                var selHasAggregates = AggregateChecker.HasAggregates(select);
                var selHasJoin = select.From is JoinExpression;
                var frmHasOrderBy = fromSelect.OrderBy != null && fromSelect.OrderBy.Count > 0;
                var frmHasGroupBy = fromSelect.GroupBy != null && fromSelect.GroupBy.Count > 0;
                var frmHasAggregates = AggregateChecker.HasAggregates(fromSelect);

                // both cannot have orderby
                if (selHasOrderBy && frmHasOrderBy)
                {
                    return false;
                }

                // both cannot have groupby
                if (selHasGroupBy && frmHasGroupBy)
                {
                    return false;
                }

                // these are distinct operations 
                if (select.IsReverse || fromSelect.IsReverse)
                {
                    return false;
                }

                // cannot move forward order-by if outer has group-by
                if (frmHasOrderBy && (selHasGroupBy || selHasAggregates || select.IsDistinct))
                {
                    return false;
                }

                // cannot move forward group-by if outer has where clause
                if (frmHasGroupBy /*&& (select.Where != null)*/) // need to assert projection is the same in order to move group-by forward
                {
                    return false;
                }

                // cannot move forward a take if outer has take or skip or distinct
                if (fromSelect.Take != null && (select.Take != null || select.Skip != null || select.IsDistinct || selHasAggregates || selHasGroupBy || selHasJoin))
                {
                    return false;
                }

                // cannot move forward a skip if outer has skip or distinct
                if (fromSelect.Skip != null && (select.Skip != null || select.IsDistinct || selHasAggregates || selHasGroupBy || selHasJoin))
                {
                    return false;
                }

                // cannot move forward a distinct if outer has take, skip, groupby or a different projection
                if (fromSelect.IsDistinct && (select.Take != null || select.Skip != null || !selHasNameMapProjection || selHasGroupBy || selHasAggregates || (selHasOrderBy && !isTopLevel) || selHasJoin))
                {
                    return false;
                }

                if (frmHasAggregates && (select.Take != null || select.Skip != null || select.IsDistinct || selHasAggregates || selHasGroupBy || selHasJoin))
                {
                    return false;
                }

                return true;
            }
        }
    }
}