// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.ObjectModel;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Rewrite aggregate expressions, moving them into same select expression that has the group-by clause
    /// </summary>
    public class AggregateRewriter : DbExpressionVisitor
    {
        private ILookup<TableAlias, AggregateSubqueryExpression> lookup;
        private Dictionary<AggregateSubqueryExpression, Expression> map;

        public static Expression Rewrite(Expression expression)
        {
            return new AggregateRewriter
                {
                    map = new Dictionary<AggregateSubqueryExpression, Expression>(),
                    lookup = AggregateGatherer.Gather(expression).ToLookup(a => a.GroupByAlias)
                }
            .Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelect(select);
            if (lookup.Contains(select.Alias))
            {
                var aggColumns = new List<ColumnDeclaration>(select.Columns);
                foreach (var ae in lookup[select.Alias])
                {
                    var name = $"agg{aggColumns.Count}";
                    var cd = new ColumnDeclaration(name, ae.AggregateInGroupSelect);
                    map.Add(ae, new ColumnExpression(ae.Type, ae.GroupByAlias, name, null));
                    aggColumns.Add(cd);
                }

                return new SelectExpression(select.Alias, aggColumns, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.Segment, select.Having, select.IsReverse);
            }

            return select;
        }

        protected override Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate)
        {
            Expression mapped;
            return map.TryGetValue(aggregate, out mapped) ? mapped : Visit(aggregate.AggregateAsSubquery);
        }

        private class AggregateGatherer : DbExpressionVisitor
        {
            private List<AggregateSubqueryExpression> aggregates = new List<AggregateSubqueryExpression>();

            public static ReadOnlyCollection<AggregateSubqueryExpression> Gather(Expression expression)
            {
                var gatherer = new AggregateGatherer();
                gatherer.Visit(expression);
                return new ReadOnlyCollection<AggregateSubqueryExpression>(gatherer.aggregates);
            }

            protected override Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate)
            {
                aggregates.Add(aggregate);
                return base.VisitAggregateSubquery(aggregate);
            }
        }
    }
}
