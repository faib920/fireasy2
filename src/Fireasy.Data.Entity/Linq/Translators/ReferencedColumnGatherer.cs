// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Gathers all columns referenced by the given expression
    /// </summary>
    public class ReferencedColumnGatherer : DbExpressionVisitor
    {
        private HashSet<ColumnExpression> columns = new HashSet<ColumnExpression>();
        private bool first = true;

        public static HashSet<ColumnExpression> Gather(Expression expression)
        {
            var visitor = new ReferencedColumnGatherer();
            visitor.Visit(expression);
            return visitor.columns;
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            this.columns.Add(column);
            return column;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            if (first)
            {
                first = false;
                return base.VisitSelect(select);
            }

            return select;
        }
    }
}