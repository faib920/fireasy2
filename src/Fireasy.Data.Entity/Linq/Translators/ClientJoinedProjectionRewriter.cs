// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// rewrites nested projections into client-side joins
    /// </summary>
    public class ClientJoinedProjectionRewriter : DbExpressionVisitor
    {
        private bool isTopLevel = true;
        private SelectExpression currentSelect;
        MemberInfo currentMember;
        bool canJoinOnClient = true;

        public static Expression Rewrite(Expression expression)
        {
            return new ClientJoinedProjectionRewriter().Visit(expression);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            var saveMember = this.currentMember;
            currentMember = assignment.Member;
            Expression e = this.Visit(assignment.Expression);
            currentMember = saveMember;
            return assignment.Update(e);
        }

        protected override Expression VisitMemberAndExpression(MemberInfo member, Expression expression)
        {
            var saveMember = this.currentMember;
            currentMember = member;
            Expression e = this.Visit(expression);
            currentMember = saveMember;
            return e;
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            var saved = currentSelect;
            currentSelect = proj.Select;
            try
            {
                if (!isTopLevel)
                {
                    if (CanJoinOnClient(currentSelect))
                    {
                        // make a query that combines all the constraints from the outer queries into a single select
                        var newOuterSelect = (SelectExpression)QueryDuplicator.Duplicate(saved);
                        
                        // remap any references to the outer select to the new alias;
                        var newInnerSelect = (SelectExpression)ColumnMapper.Map(proj.Select, newOuterSelect.Alias, saved.Alias);
                        // add outer-join test
                        var newInnerProjection = new ProjectionExpression(newInnerSelect, proj.Projector, proj.IsAsync).AddOuterJoinTest();
                        newInnerSelect = newInnerProjection.Select;
                        var newProjector = newInnerProjection.Projector;

                        var newAlias = new TableAlias();
                        var pc = ColumnProjector.ProjectColumns(QueryUtility.CanBeColumnExpression, newProjector, newOuterSelect.Columns, newAlias, newOuterSelect.Alias, newInnerSelect.Alias);
                        var join = new JoinExpression(JoinType.OuterApply, newOuterSelect, newInnerSelect, null);
                        var joinedSelect = new SelectExpression(newAlias, pc.Columns, join, null, null, null, proj.IsSingleton, null, null, null, null, false);

                        // apply client-join treatment recursively
                        currentSelect = joinedSelect;
                        newProjector = Visit(pc.Projector);

                        // compute keys (this only works if join condition was a single column comparison)
                        var outerKeys = new List<Expression>();
                        var innerKeys = new List<Expression>();

                        if (GetEquiJoinKeyExpressions(newInnerSelect.Where, newOuterSelect.Alias, outerKeys, innerKeys))
                        {
                            // outerKey needs to refer to the outer-scope's alias
                            var outerKey = outerKeys.Select(k => ColumnMapper.Map(k, saved.Alias, newOuterSelect.Alias));
                            // innerKey needs to refer to the new alias for the select with the new join
                            var innerKey = innerKeys.Select(k => ColumnMapper.Map(k, joinedSelect.Alias, ((ColumnExpression)k).Alias));
                            var newProjection = new ProjectionExpression(joinedSelect, newProjector, proj.Aggregator, proj.IsAsync);
                            return new ClientJoinExpression(newProjection, outerKey, innerKey);
                        }
                    }
                }
                else
                {
                    isTopLevel = false;
                }

                return base.VisitProjection(proj);
            }
            finally
            {
                currentSelect = saved;
            }
        }

        private bool CanJoinOnClient(SelectExpression select)
        {
            // can add singleton (1:0,1) join if no grouping/aggregates or distinct
            return
                this.canJoinOnClient
                && this.currentMember != null
                && !select.IsDistinct
                && (select.GroupBy == null || select.GroupBy.Count == 0)
                && !AggregateChecker.HasAggregates(select);
        }

        private bool GetEquiJoinKeyExpressions(Expression predicate, TableAlias outerAlias, List<Expression> outerExpressions, List<Expression> innerExpressions)
        {
            var b = predicate as BinaryExpression;
            if (b != null)
            {
                switch (predicate.NodeType)
                {
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                        return GetEquiJoinKeyExpressions(b.Left, outerAlias, outerExpressions, innerExpressions)
                            && GetEquiJoinKeyExpressions(b.Right, outerAlias, outerExpressions, innerExpressions);
                    case ExpressionType.Equal:
                        var left = b.Left as ColumnExpression;
                        var right = b.Right as ColumnExpression;
                        if (left != null && right != null)
                        {
                            if (left.Alias == outerAlias)
                            {
                                outerExpressions.Add(left);
                                innerExpressions.Add(right);
                                return true;
                            }
                            else if (right.Alias == outerAlias)
                            {
                                innerExpressions.Add(left);
                                outerExpressions.Add(right);
                                return true;
                            }
                        }
                        break;
                }
            }
            return false;
        }

        private ColumnExpression GetColumnExpression(Expression expression)
        {
            // ignore converions 
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression as ColumnExpression;
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
