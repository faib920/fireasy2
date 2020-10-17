// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Rewrite all column references to one or more aliases to a new single alias
    /// </summary>
    public class ColumnMapper : DbExpressionVisitor
    {
        private readonly HashSet<TableAlias> _oldAliases;
        private readonly TableAlias _newAlias;

        private ColumnMapper(IEnumerable<TableAlias> oldAliases, TableAlias newAlias)
        {
            _oldAliases = new HashSet<TableAlias>(oldAliases);
            _newAlias = newAlias;
        }

        internal static Expression Map(Expression expression, TableAlias newAlias, params TableAlias[] oldAliases)
        {
            return new ColumnMapper((IEnumerable<TableAlias>)oldAliases, newAlias).Visit(expression);
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (_oldAliases.Contains(column.Alias))
            {
                return new ColumnExpression(column.Type, _newAlias, column.Name, column.MapInfo);
            }

            return column;
        }
    }
}
