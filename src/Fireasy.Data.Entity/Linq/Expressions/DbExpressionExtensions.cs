// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)


using Fireasy.Data.Entity.Linq.Translators;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fireasy.Common.Extensions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表达式的内部的扩展方法。
    /// </summary>
    internal static class DbExpressionExtensions
    {
        internal static SelectExpression SetColumns(this SelectExpression select, IEnumerable<ColumnDeclaration> columns)
        {
            return new SelectExpression(select.Alias, columns.OrderBy(c => c.Name), select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.Segment, select.Having, select.IsReverse);
        }

        internal static SelectExpression AddColumn(this SelectExpression select, ColumnDeclaration column)
        {
            var columns = new List<ColumnDeclaration>(select.Columns) {column};
            return select.SetColumns(columns);
        }

        internal static SelectExpression RemoveColumn(this SelectExpression select, ColumnDeclaration column)
        {
            var columns = new List<ColumnDeclaration>(select.Columns);
            columns.Remove(column);
            return select.SetColumns(columns);
        }

        internal static string GetAvailableColumnName(this SelectExpression select, string baseName)
        {
            var name = baseName;
            var n = 0;
            while (!IsUniqueName(select, name))
            {
                name = baseName + (n++);
            }
            return name;
        }

        private static bool IsUniqueName(SelectExpression select, string name)
        {
            return select.Columns.All(col => col.Name != name);
        }

        internal static ProjectionExpression AddOuterJoinTest(this ProjectionExpression proj, string name = null)
        {
            var test = GetOuterJoinTest(proj.Select);
            var select = proj.Select;
            ColumnExpression testCol = null;
            // look to see if test expression exists in columns already
            foreach (var col in select.Columns)
            {
                if (test.Equals(col.Expression))
                {
                    testCol = new ColumnExpression(test.Type, select.Alias, col.Name, null);
                    break;
                }
            }
            if (testCol == null)
            {
                // add expression to projection
                testCol = test as ColumnExpression;
                string colName = (testCol != null) ? testCol.Name : "Test";
                colName = proj.Select.Columns.GetAvailableColumnName(colName);
                select = select.AddColumn(new ColumnDeclaration(colName, test));
                testCol = new ColumnExpression(test.Type, select.Alias, colName, null);
            }
            var newProjector = new OuterJoinedExpression(testCol, proj.Projector);
            return new ProjectionExpression(select, newProjector, proj.Aggregator, proj.IsAsync);
        }

        internal static SelectExpression SetDistinct(this SelectExpression select, bool isDistinct)
        {
            return select.IsDistinct != isDistinct ? new SelectExpression(select.Alias, select.Columns, select.From, select.Where, select.OrderBy, select.GroupBy, isDistinct, select.Skip, select.Take, select.Segment, select.Having, select.IsReverse) : select;
        }

        internal static SelectExpression SetWhere(this SelectExpression select, Expression where)
        {
            return where != select.Where ? new SelectExpression(select.Alias, select.Columns, select.From, where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.Segment, select.Having, select.IsReverse) : select;
        }

        internal static SelectExpression SetOrderBy(this SelectExpression select, IEnumerable<OrderExpression> orderBy)
        {
            return new SelectExpression(select.Alias, select.Columns, select.From, select.Where, orderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.Segment, select.Having, select.IsReverse);
        }

        internal static SelectExpression AddOrderExpression(this SelectExpression select, OrderExpression ordering)
        {
            var orderby = new List<OrderExpression>();
            if (select.OrderBy != null)
                orderby.AddRange(select.OrderBy);
            orderby.Add(ordering);
            return select.SetOrderBy(orderby);
        }

        internal static SelectExpression RemoveOrderExpression(this SelectExpression select, OrderExpression ordering)
        {
            if (select.OrderBy != null && select.OrderBy.Count > 0)
            {
                var orderby = new List<OrderExpression>(select.OrderBy);
                orderby.Remove(ordering);
                return select.SetOrderBy(orderby);
            }
            return select;
        }

        internal static SelectExpression SetGroupBy(this SelectExpression select, IEnumerable<Expression> groupBy)
        {
            return new SelectExpression(select.Alias, select.Columns, select.From, select.Where, select.OrderBy, groupBy, select.IsDistinct, select.Skip, select.Take, select.Segment, select.Having, select.IsReverse);
        }

        internal static SelectExpression AddGroupExpression(this SelectExpression select, Expression expression)
        {
            var groupby = new List<Expression>();
            if (select.GroupBy != null)
                groupby.AddRange(select.GroupBy);
            groupby.Add(expression);
            return select.SetGroupBy(groupby);
        }

        internal static SelectExpression RemoveGroupExpression(this SelectExpression select, Expression expression)
        {
            if (select.GroupBy != null && select.GroupBy.Count > 0)
            {
                var groupby = new List<Expression>(select.GroupBy);
                groupby.Remove(expression);
                return select.SetGroupBy(groupby);
            }
            return select;
        }

        internal static SelectExpression SetSkip(this SelectExpression select, Expression skip)
        {
            return skip != select.Skip ? new SelectExpression(select.Alias, select.Columns, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, skip, select.Take, select.Segment, select.Having, select.IsReverse) : select;
        }

        internal static SelectExpression SetTake(this SelectExpression select, Expression take)
        {
            return take != select.Take ? new SelectExpression(select.Alias, select.Columns, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, take, select.Segment, select.Having, select.IsReverse) : select;
        }

        internal static SelectExpression AddRedundantSelect(this SelectExpression select, TableAlias newAlias)
        {
            var newColumns = select.Columns.Select(d =>
                new ColumnDeclaration(d.Name,
                    new ColumnExpression(
                        d.Expression.Type,
                        newAlias, d.Name, 
                        d.Expression is ColumnExpression ? ((ColumnExpression)d.Expression).MapInfo : null
                        )));
            var newFrom = new SelectExpression(newAlias, select.Columns, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.Segment, select.Having, select.IsReverse);
            return new SelectExpression(select.Alias, newColumns, newFrom, null, null, null, false, null, null, null, null, false);
        }

        internal static SelectExpression SetFrom(this SelectExpression select, Expression from)
        {
            return select.From != from ? new SelectExpression(select.Alias, select.Columns, from, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.Segment, select.Having, select.IsReverse) : select;
        }

        private static Expression GetOuterJoinTest(SelectExpression select)
        {
            var aliases = DeclaredAliasGatherer.Gather(select.From);
            var joinColumns = JoinColumnGatherer.Gather(aliases, select).ToList();
            if (joinColumns.Count > 0)
            {
                // prefer one that is already in the projection list.
                foreach (var jc in joinColumns)
                {
                    foreach (var col in select.Columns)
                    {
                        if (jc.Equals(col.Expression))
                        {
                            return jc;
                        }
                    }
                }
                return joinColumns[0];
            }

            // fall back to introducing a constant
            return Expression.Constant(1, typeof(int?));
        }

    }
}
