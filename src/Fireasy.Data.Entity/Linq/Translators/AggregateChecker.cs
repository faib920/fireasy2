﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Determines if a SelectExpression contains any aggregate expressions
    /// </summary>
    public class AggregateChecker : DbExpressionVisitor
    {
        private bool _hasAggregate;

        public static bool HasAggregates(SelectExpression expression)
        {
            var checker = new AggregateChecker();
            checker.Visit(expression);
            return checker._hasAggregate;
        }

        protected override Expression VisitAggregate(AggregateExpression aggregate)
        {
            _hasAggregate = true;
            return aggregate;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            // only consider aggregates in these locations
            Visit(select.Where);
            VisitOrderBy(select.OrderBy);
            VisitColumnDeclarations(select.Columns);

            return select;
        }

        protected override Expression VisitSubquery(SubqueryExpression subquery)
        {
            // don't count aggregates in subqueries
            return subquery;
        }
    }
}
