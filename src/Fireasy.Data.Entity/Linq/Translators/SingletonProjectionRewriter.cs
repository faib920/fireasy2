// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Linq.Expressions;
using System.Reflection;
using Fireasy.Data.Entity.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Rewrites nested singleton projection into server-side joins
    /// </summary>
    public class SingletonProjectionRewriter : DbExpressionVisitor
    {
        private bool isTopLevel = true;
        private SelectExpression currentSelect;

        public static Expression Rewrite(Expression expression)
        {
            return new SingletonProjectionRewriter().Visit(expression);
        }

        protected override Expression VisitClientJoin(ClientJoinExpression join)
        {
            // treat client joins as new top level
            var saveTop = this.isTopLevel;
            var saveSelect = this.currentSelect;
            isTopLevel = true;
            currentSelect = null;

            Expression result = base.VisitClientJoin(join);

            isTopLevel = saveTop;
            currentSelect = saveSelect;

            return result;
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            if (isTopLevel)
            {
                isTopLevel = false;
                currentSelect = proj.Select;
                var projector = Visit(proj.Projector);

                if (projector != proj.Projector || currentSelect != proj.Select)
                {
                    return new ProjectionExpression(currentSelect, projector, proj.Aggregator, proj.IsAsync);
                }

                return proj;
            }

            if (proj.IsSingleton && CanJoinOnServer(currentSelect))
            {
                var newAlias = new TableAlias();
                currentSelect = currentSelect.AddRedundantSelect(newAlias);
                var source = (SelectExpression)ColumnMapper.Map(proj.Select, newAlias, currentSelect.Alias);
                var pex = new ProjectionExpression(source, proj.Projector, proj.IsAsync).AddOuterJoinTest();
                var pc = ColumnProjector.ProjectColumns(QueryUtility.CanBeColumnExpression, pex.Projector, currentSelect.Columns, currentSelect.Alias, newAlias, proj.Select.Alias);

                // **fix** 解决返回关联对象后使用Distinct的问题
                var join = CreateJoinExpression(currentSelect.From, pex.Select, out bool isDistinct);
                currentSelect = new SelectExpression(currentSelect.Alias, pc.Columns, join, null);
                if (isDistinct)
                {
                    var newPc = ColumnProjector.ProjectColumns(QueryUtility.CanBeColumnExpression, proj.Projector, null, currentSelect.Alias, proj.Select.Alias);
                    currentSelect = new SelectExpression(currentSelect.Alias, newPc.Columns, currentSelect, null, null, null, isDistinct, null, null, null, null, false);
                    return Visit(newPc.Projector);
                }

                return Visit(pc.Projector);
            }

            var saveTop = isTopLevel;
            var saveSelect = currentSelect;
            isTopLevel = true;
            currentSelect = null;

            var result = base.VisitProjection(proj);

            isTopLevel = saveTop;
            currentSelect = saveSelect;

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
            isTopLevel = true;
            return base.VisitCommand(command);
        }
    }
}