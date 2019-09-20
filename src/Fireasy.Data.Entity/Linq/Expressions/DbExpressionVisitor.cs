// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 一个抽象类，对 ELinq 表达式树进行访问。
    /// </summary>
    public abstract class DbExpressionVisitor : Common.Linq.Expressions.ExpressionVisitor
    {
        /// <summary>
        /// 访问 <see cref="Expression"/>。
        /// </summary>
        /// <param name="exp">要访问的表达式。</param>
        /// <returns></returns>
        public override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }
            switch ((DbExpressionType)exp.NodeType)
            {
                case DbExpressionType.Table:
                    return VisitTable((TableExpression)exp);
                case DbExpressionType.ClientJoin:
                    return VisitClientJoin((ClientJoinExpression)exp);
                case DbExpressionType.Column:
                    return VisitColumn((ColumnExpression)exp);
                case DbExpressionType.Select:
                    return VisitSelect((SelectExpression)exp);
                case DbExpressionType.Join:
                    return VisitJoin((JoinExpression)exp);
                case DbExpressionType.OuterJoined:
                    return VisitOuterJoined((OuterJoinedExpression)exp);
                case DbExpressionType.Aggregate:
                    return VisitAggregate((AggregateExpression)exp);
                case DbExpressionType.Scalar:
                case DbExpressionType.Exists:
                case DbExpressionType.In:
                    return VisitSubquery((SubqueryExpression)exp);
                case DbExpressionType.AggregateSubquery:
                    return VisitAggregateSubquery((AggregateSubqueryExpression)exp);
                case DbExpressionType.IsNull:
                    return VisitIsNull((IsNullExpression)exp);
                case DbExpressionType.Between:
                    return VisitBetween((BetweenExpression)exp);
                case DbExpressionType.RowCount:
                    return VisitRowNumber((RowNumberExpression)exp);
                case DbExpressionType.Projection:
                    return VisitProjection((ProjectionExpression)exp);
                case DbExpressionType.NamedValue:
                    return this.VisitNamedValue((NamedValueExpression)exp);
                case DbExpressionType.Function:
                    return VisitFunction((FunctionExpression)exp);
                case DbExpressionType.Entity:
                    return VisitEntity((EntityExpression)exp);
                case DbExpressionType.Segment:
                    return VisitSegment((SegmentExpression)exp);
                case DbExpressionType.Delete:
                case DbExpressionType.Update:
                case DbExpressionType.Insert:
                case DbExpressionType.Block:
                    return VisitCommand((CommandExpression)exp);
                case DbExpressionType.Batch:
                    return VisitBatch((BatchCommandExpression)exp);
                case DbExpressionType.Variable:
                    return VisitVariable((VariableExpression)exp);
                case DbExpressionType.Generator:
                    return VisitGenerator((GeneratorExpression)exp);
                case DbExpressionType.SqlText:
                    return VisitSqlText((SqlExpression)exp);
                case DbExpressionType.CaseWhen:
                    return VisitCaseWhen((CaseWhenExpression)exp);
                default:
                    return base.Visit(exp);
            }
        }

        /// <summary>
        /// 访问 <see cref="EntityExpression"/>。
        /// </summary>
        /// <param name="entity">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitEntity(EntityExpression entity)
        {
            var exp = Visit(entity.Expression);
            return entity.Update(exp, entity.IsNoTracking);
        }

        /// <summary>
        /// 访问 <see cref="TableExpression"/>。
        /// </summary>
        /// <param name="table">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitTable(TableExpression table)
        {
            return table;
        }

        /// <summary>
        /// 访问 <see cref="ColumnExpression"/>。
        /// </summary>
        /// <param name="column">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitColumn(ColumnExpression column)
        {
            return column;
        }

        /// <summary>
        /// 访问 <see cref="SelectExpression"/>。
        /// </summary>
        /// <param name="select">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitSelect(SelectExpression select)
        {
            var from = VisitSource(select.From);
            var where = Visit(select.Where);
            var orderBy = VisitOrderBy(select.OrderBy);
            var groupBy = VisitMemberAndExpressionList(select.GroupBy);
            var skip = Visit(select.Skip);
            var take = Visit(select.Take);
            var segment = Visit(select.Segment);
            var columns = VisitColumnDeclarations(select.Columns);
            var having = Visit(select.Having);
            return select.Update(from, where, orderBy, groupBy, skip, take, segment, select.IsDistinct, columns, having, select.IsReverse);
        }

        /// <summary>
        /// 访问 <see cref="SegmentExpression"/>。
        /// </summary>
        /// <param name="segment">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitSegment(SegmentExpression segment)
        {
            return segment;
        }

        /// <summary>
        /// 访问 <see cref="JoinExpression"/>。
        /// </summary>
        /// <param name="join">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitJoin(JoinExpression join)
        {
            var left = VisitSource(join.Left);
            var right = VisitSource(join.Right);
            var condition = Visit(join.Condition);
            return join.Update(join.JoinType, left, right, condition);
        }

        /// <summary>
        /// 访问 <see cref="OuterJoinedExpression"/>。
        /// </summary>
        /// <param name="outer">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitOuterJoined(OuterJoinedExpression outer)
        {
            var test = Visit(outer.Test);
            var expression = Visit(outer.Expression);
            return outer.Update(test, expression);
        }

        protected virtual Expression VisitClientJoin(ClientJoinExpression join)
        {
            var projection = (ProjectionExpression)this.Visit(join.Projection);
            var outerKey = this.VisitMemberAndExpressionList(join.OuterKey);
            var innerKey = this.VisitMemberAndExpressionList(join.InnerKey);
            return join.Update(projection, outerKey, innerKey);
        }

        /// <summary>
        /// 访问 <see cref="AggregateExpression"/>。
        /// </summary>
        /// <param name="aggregate">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitAggregate(AggregateExpression aggregate)
        {
            var arg = Visit(aggregate.Argument);
            return aggregate.Update(aggregate.Type, aggregate.AggregateType, arg, aggregate.IsDistinct);
        }

        /// <summary>
        /// 访问 <see cref="IsNullExpression"/>。
        /// </summary>
        /// <param name="isnull">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitIsNull(IsNullExpression isnull)
        {
            var expr = Visit(isnull.Expression);
            return isnull.Update(expr);
        }

        /// <summary>
        /// 访问 <see cref="BetweenExpression"/>。
        /// </summary>
        /// <param name="between">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitBetween(BetweenExpression between)
        {
            var expr = Visit(between.Argument);
            var lower = Visit(between.Lower);
            var upper = Visit(between.Upper);
            return between.Update(expr, lower, upper);
        }

        /// <summary>
        /// 访问 <see cref="RowNumberExpression"/>。
        /// </summary>
        /// <param name="rowNumber">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitRowNumber(RowNumberExpression rowNumber)
        {
            var orderby = VisitOrderBy(rowNumber.OrderBy);
            return rowNumber.Update(orderby);
        }

        protected virtual Expression VisitNamedValue(NamedValueExpression value)
        {
            return value;
        }

        /// <summary>
        /// 访问 <see cref="SubqueryExpression"/>。
        /// </summary>
        /// <param name="subquery">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitSubquery(SubqueryExpression subquery)
        {
            switch (subquery.NodeType)
            {
                case DbExpressionType.Scalar:
                    return VisitScalar((ScalarExpression)subquery);
                case DbExpressionType.Exists:
                    return VisitExists((ExistsExpression)subquery);
                case DbExpressionType.In:
                    return VisitIn((InExpression)subquery);
            }
            return subquery;
        }

        /// <summary>
        /// 访问 <see cref="ScalarExpression"/>。
        /// </summary>
        /// <param name="scalar">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitScalar(ScalarExpression scalar)
        {
            var select = (SelectExpression)Visit(scalar.Select);
            return scalar.Update(select);
        }

        /// <summary>
        /// 访问 <see cref="ExistsExpression"/>。
        /// </summary>
        /// <param name="exists">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitExists(ExistsExpression exists)
        {
            var select = (SelectExpression)Visit(exists.Select);
            return exists.Update(select);
        }

        /// <summary>
        /// 访问 <see cref="InExpression"/>。
        /// </summary>
        /// <param name="in">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitIn(InExpression @in)
        {
            var expr = Visit(@in.Expression);
            var select = (SelectExpression)Visit(@in.Select);
            var values = VisitMemberAndExpressionList(@in.Values);
            return @in.Update(expr, select, values);
        }

        /// <summary>
        /// 访问 <see cref="AggregateSubqueryExpression"/>。
        /// </summary>
        /// <param name="aggregate">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate)
        {
            var subquery = (ScalarExpression) Visit(aggregate.AggregateAsSubquery);
            return aggregate.Update(subquery);
        }

        /// <summary>
        /// 访问 <see cref="Expression"/>。
        /// </summary>
        /// <param name="source">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitSource(Expression source)
        {
            return Visit(source);
        }

        /// <summary>
        /// 访问 <see cref="ProjectionExpression"/>。
        /// </summary>
        /// <param name="proj">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitProjection(ProjectionExpression proj)
        {
            var select = (SelectExpression)Visit(proj.Select);
            var projector = Visit(proj.Projector);
            return proj.Update(select, projector, proj.Aggregator);
        }

        protected virtual Expression VisitVariable(VariableExpression vex)
        {
            return vex;
        }

        /// <summary>
        /// 访问 <see cref="FunctionExpression"/>。
        /// </summary>
        /// <param name="func">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitFunction(FunctionExpression func)
        {
            var arguments = VisitMemberAndExpressionList(func.Arguments);
            return func.Update(func.Name, arguments);
        }

        protected virtual Expression VisitCommand(CommandExpression command)
        {
            switch ((DbExpressionType)command.NodeType)
            {
                case DbExpressionType.Insert:
                    return this.VisitInsert((InsertCommandExpression)command);
                case DbExpressionType.Update:
                    return this.VisitUpdate((UpdateCommandExpression)command);
                case DbExpressionType.Delete:
                    return this.VisitDelete((DeleteCommandExpression)command);
                case DbExpressionType.Block:
                    return this.VisitBlock((BlockCommandExpression)command);
                default:
                    return this.VisitUnknown(command);
            }
        }

        /// <summary>
        /// 访问 <see cref="BatchCommandExpression"/>。
        /// </summary>
        /// <param name="batch">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitBatch(BatchCommandExpression batch)
        {
            var operation = (LambdaExpression)this.Visit(batch.Operation);
            return batch.Update(batch.Input, operation, batch.Arguments);
        }

        /// <summary>
        /// 访问 <see cref="DeleteCommandExpression"/>。
        /// </summary>
        /// <param name="delete">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitDelete(DeleteCommandExpression delete)
        {
            var table = Visit(delete.Table);
            var where = Visit(delete.Where);
            return delete.Update(table, where);
        }

        /// <summary>
        /// 访问 <see cref="UpdateCommandExpression"/>。
        /// </summary>
        /// <param name="update">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitUpdate(UpdateCommandExpression update)
        {
            var table = Visit(update.Table);
            var where = Visit(update.Where);
            var assignments = this.VisitColumnAssignments(update.Assignments);
            return update.Update(table, where, assignments);
        }

        /// <summary>
        /// 访问 <see cref="InsertCommandExpression"/>。
        /// </summary>
        /// <param name="insert">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitInsert(InsertCommandExpression insert)
        {
            var table = Visit(insert.Table);
            var assignments = this.VisitColumnAssignments(insert.Assignments);
            return insert.Update(table, assignments);
        }

        /// <summary>
        /// 访问 <see cref="BlockCommandExpression"/>。
        /// </summary>
        /// <param name="block">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitBlock(BlockCommandExpression block)
        {
            return block;
        }

        protected virtual Expression VisitGenerator(GeneratorExpression generator)
        {
            return generator;
        }

        protected virtual Expression VisitSqlText(SqlExpression sql)
        {
            return sql;
        }

        protected virtual Expression VisitCaseWhen(CaseWhenExpression caseWhen)
        {
            return caseWhen;
        }

        protected virtual ReadOnlyCollection<ColumnAssignment> VisitColumnAssignments(ReadOnlyCollection<ColumnAssignment> assignments)
        {
            List<ColumnAssignment> alternate = null;
            for (int i = 0, n = assignments.Count; i < n; i++)
            {
                ColumnAssignment assignment = this.VisitColumnAssignment(assignments[i]);
                if (alternate == null && assignment != assignments[i])
                {
                    alternate = assignments.Take(i).ToList();
                }
                if (alternate != null)
                {
                    alternate.Add(assignment);
                }
            }
            if (alternate != null)
            {
                return alternate.AsReadOnly();
            }
            return assignments;
        }

        /// <summary>
        /// 访问 <see cref="ColumnDeclaration"/>集合。
        /// </summary>
        /// <param name="columns">要访问的集合。</param>
        /// <returns></returns>
        protected virtual ReadOnlyCollection<ColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            List<ColumnDeclaration> alternate = null;
            var count = columns.Count;
            for (int i = 0, n = count; i < n; i++)
            {
                var column = columns[i];
                var e = Visit(column.Expression);
                if (alternate == null && e != column.Expression)
                {
                    alternate = columns.Take(i).ToList();
                }
                if (alternate != null)
                {
                    alternate.Add(new ColumnDeclaration(column.Name, e));
                }
            }
            if (alternate != null)
            {
                return alternate.AsReadOnly();
            }
            return columns;
        }

        protected virtual ColumnAssignment VisitColumnAssignment(ColumnAssignment ca)
        {
            ColumnExpression c = (ColumnExpression)this.Visit(ca.Column);
            Expression e = this.Visit(ca.Expression);
            return this.UpdateColumnAssignment(ca, c, e);
        }

        /// <summary>
        /// 访问 <see cref="OrderExpression"/>集合。
        /// </summary>
        /// <param name="expressions">要访问的表达式集合。</param>
        /// <returns></returns>
        protected virtual ReadOnlyCollection<OrderExpression> VisitOrderBy(ReadOnlyCollection<OrderExpression> expressions)
        {
            if (expressions != null)
            {
                List<OrderExpression> alternate = null;
                var count = expressions.Count;
                for (int i = 0, n = count; i < n; i++)
                {
                    var expr = expressions[i];
                    var e = Visit(expr.Expression);
                    if (alternate == null && e != expr.Expression)
                    {
                        alternate = expressions.Take(i).ToList();
                    }
                    if (alternate != null)
                    {
                        alternate.Add(new OrderExpression(expr.OrderType, e));
                    }
                }
                if (alternate != null)
                {
                    return alternate.AsReadOnly();
                }
            }
            return expressions;
        }

        protected ColumnAssignment UpdateColumnAssignment(ColumnAssignment ca, ColumnExpression c, Expression e)
        {
            if (c != ca.Column || e != ca.Expression)
            {
                return new ColumnAssignment(c, e);
            }
            return ca;
        }
    }
}
