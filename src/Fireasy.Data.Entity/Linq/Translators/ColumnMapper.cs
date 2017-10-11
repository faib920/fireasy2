// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;
using System.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Rewrite all column references to one or more aliases to a new single alias
    /// </summary>
    public class ColumnMapper : DbExpressionVisitor
    {
        private HashSet<TableAlias> oldAliases;
        private TableAlias newAlias;

        private ColumnMapper(IEnumerable<TableAlias> oldAliases, TableAlias newAlias)
        {
            this.oldAliases = new HashSet<TableAlias>(oldAliases);
            this.newAlias = newAlias;
        }

        internal static Expression Map(Expression expression, TableAlias newAlias, params TableAlias[] oldAliases)
        {
            return new ColumnMapper((IEnumerable<TableAlias>)oldAliases, newAlias).Visit(expression);
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (oldAliases.Contains(column.Alias))
            {
                return new ColumnExpression(column.Type, newAlias, column.Name, column.MapInfo);
            }

            return column;
        }
    }
}
