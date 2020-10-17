// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Rewrites nested singleton projection into server-side joins
    /// </summary>
    public class SingletonProjectionRewriter : DbExpressionVisitor
    {
        private bool _isTopLevel = true;
        private SelectExpression _currentSelect;

        public static Expression Rewrite(Expression expression)
        {
            return new SingletonProjectionRewriter().Visit(expression);
        }

        protected override Expression VisitClientJoin(ClientJoinExpression join)
        {
            // treat client joins as new top level
            var saveTop = _isTopLevel;
            var saveSelect = _currentSelect;
            _isTopLevel = true;
            _currentSelect = null;

            Expression result = base.VisitClientJoin(join);

            _isTopLevel = saveTop;
            _currentSelect = saveSelect;

            return result;
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            if (_isTopLevel)
            {
                _isTopLevel = false;
                _currentSelect = proj.Select;
                var projector = Visit(proj.Projector);

                if (projector != proj.Projector || _currentSelect != proj.Select)
                {
                    return new ProjectionExpression(_currentSelect, projector, proj.Aggregator, proj.IsAsync, proj.IsNoTracking);
                }

                return proj;
            }

            if (proj.IsSingleton && CanJoinOnServer(_currentSelect))
            {
                var newAlias = new TableAlias();
                _currentSelect = _currentSelect.AddRedundantSelect(newAlias);
                var source = (SelectExpression)ColumnMapper.Map(proj.Select, newAlias, _currentSelect.Alias);
                var pex = new ProjectionExpression(source, proj.Projector, proj.IsAsync, proj.IsNoTracking).AddOuterJoinTest();
                var pc = ColumnProjector.ProjectColumns(QueryUtility.CanBeColumnExpression, pex.Projector, _currentSelect.Columns, _currentSelect.Alias, newAlias, proj.Select.Alias);

                // **fix** 解决返回关联对象后使用Distinct的问题
                var join = CreateJoinExpression(_currentSelect.From, pex.Select, out bool isDistinct);
                _currentSelect = new SelectExpression(_currentSelect.Alias, pc.Columns, join, null);
                if (isDistinct)
                {
                    var newPc = ColumnProjector.ProjectColumns(QueryUtility.CanBeColumnExpression, proj.Projector, null, _currentSelect.Alias, proj.Select.Alias);
                    _currentSelect = new SelectExpression(_currentSelect.Alias, newPc.Columns, _currentSelect, null, null, null, isDistinct, null, null, null, null, false);
                    return Visit(newPc.Projector);
                }

                return Visit(pc.Projector);
            }

            var saveTop = _isTopLevel;
            var saveSelect = _currentSelect;
            _isTopLevel = true;
            _currentSelect = null;

            var result = base.VisitProjection(proj);

            _isTopLevel = saveTop;
            _currentSelect = saveSelect;

            return result;
        }

        private bool CanJoinOnServer(SelectExpression select)
        {
            // can add singleton (1:0,1) join if no grouping/aggregates or distinct
            // **fix** !IsDistinct
            return (select.GroupBy == null || select.GroupBy.Count == 0)
                && !AggregateChecker.HasAggregates(select);
        }

        private JoinExpression CreateJoinExpression(Expression left, Expression right, out bool isDistinct)
        {
            isDistinct = false;

            if (left is SelectExpression select && select.IsDistinct)
            {
                isDistinct = true;
                left = select.Update(select.From, select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take, select.Segment, false, select.Columns, select.Having, select.IsReverse);
            }

            select = right as SelectExpression;
            if (select != null && select.IsDistinct)
            {
                isDistinct = true;
                right = select.Update(select.From, select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take, select.Segment, false, select.Columns, select.Having, select.IsReverse);
            }

            return new JoinExpression(JoinType.OuterApply, left, right, null);
        }

        protected override Expression VisitSubquery(SubqueryExpression subquery)
        {
            return subquery;
        }

        protected override Expression VisitCommand(CommandExpression command)
        {
            _isTopLevel = true;
            return base.VisitCommand(command);
        }
    }
}