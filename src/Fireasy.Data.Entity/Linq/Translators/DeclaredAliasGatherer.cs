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
    public class DeclaredAliasGatherer : DbExpressionVisitor
    {
        private readonly HashSet<TableAlias> _aliases;

        private DeclaredAliasGatherer()
        {
            _aliases = new HashSet<TableAlias>();
        }

        public static HashSet<TableAlias> Gather(Expression source)
        {
            var gatherer = new DeclaredAliasGatherer();
            gatherer.Visit(source);
            return gatherer._aliases;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            _aliases.Add(select.Alias);
            return select;
        }

        protected override Expression VisitTable(TableExpression table)
        {
            _aliases.Add(table.Alias);
            return table;
        }
    }
}

