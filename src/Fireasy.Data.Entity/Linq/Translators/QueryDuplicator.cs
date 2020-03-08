// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;
using System.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Duplicate the query expression by making a copy with new table aliases
    /// </summary>
    public class QueryDuplicator : DbExpressionVisitor
    {
        private readonly Dictionary<TableAlias, TableAlias> map = new Dictionary<TableAlias, TableAlias>();

        internal static Expression Duplicate(Expression expression)
        {
            return new QueryDuplicator().Visit(expression);
        }

        protected override Expression VisitTable(TableExpression table)
        {
            TableAlias newAlias = new TableAlias();
            map[table.Alias] = newAlias;

            return new TableExpression(newAlias, table.Name, table.Type);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            TableAlias newAlias = new TableAlias();
            map[select.Alias] = newAlias;
            select = (SelectExpression)base.VisitSelect(select);

            return new SelectExpression(newAlias, select.Columns, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.Segment, select.Having, select.IsReverse);
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (map.TryGetValue(column.Alias, out TableAlias newAlias))
            {
                return new ColumnExpression(column.Type, newAlias, column.Name, column.MapInfo);
            }

            return column;
        }
    }
}
