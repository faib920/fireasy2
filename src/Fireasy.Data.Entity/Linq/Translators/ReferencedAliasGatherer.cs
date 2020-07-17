// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    ///  returns the set of all aliases produced by a query source
    /// </summary>
    public class ReferencedAliasGatherer : DbExpressionVisitor
    {
        private readonly HashSet<TableAlias> _aliases;

        private ReferencedAliasGatherer()
        {
            _aliases = new HashSet<TableAlias>();
        }

        public static HashSet<TableAlias> Gather(Expression expression)
        {
            var gatherer = new ReferencedAliasGatherer();
            gatherer.Visit(expression);
            return gatherer._aliases;
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            _aliases.Add(column.Alias);
            return column;
        }
    }
}
