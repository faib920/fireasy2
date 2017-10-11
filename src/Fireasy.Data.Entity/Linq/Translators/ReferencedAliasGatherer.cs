// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;
using System.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    ///  returns the set of all aliases produced by a query source
    /// </summary>
    public class ReferencedAliasGatherer : DbExpressionVisitor
    {
        private readonly HashSet<TableAlias> aliases;

        private ReferencedAliasGatherer()
        {
            aliases = new HashSet<TableAlias>();
        }

        public static HashSet<TableAlias> Gather(Expression expression)
        {
            var gatherer = new ReferencedAliasGatherer();
            gatherer.Visit(expression);
            return gatherer.aliases;
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            aliases.Add(column.Alias);
            return column;
        }
    }
}
