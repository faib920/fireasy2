// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Rewrite aggregate expressions, moving them into same select expression that has the group-by clause
    /// </summary>
    public class AggregateRewriter : DbExpressionVisitor
    {
        private ILookup<TableAlias, AggregateSubqueryExpression> _lookup;
        private Dictionary<AggregateSubqueryExpression, Expression> _map;

        public static Expression Rewrite(Expression expression)
        {
            return new AggregateRewriter
            {
                _map = new Dictionary<AggregateSubqueryExpression, Expression>(),
                _lookup = AggregateGatherer.Gather(expression).ToLookup(a => a.GroupByAlias)
            }
            .Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelect(select);
            if (_lookup.Contains(select.Alias))
            {
                var aggColumns = new List<ColumnDeclaration>(select.Columns);
                foreach (var ae in _lookup[select.Alias])
                {
                    var name = $"agg{aggColumns.Count}";
                    var cd = new ColumnDeclaration(name, ae.AggregateInGroupSelect);
                    _map.Add(ae, new ColumnExpression(ae.Type, ae.GroupByAlias, name, null));
                    aggColumns.Add(cd);
                }

                return new SelectExpression(select.Alias, aggColumns, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take, select.Segment, select.Having, select.IsReverse);
            }

            return select;
        }

        protected override Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate)
        {
            return _map.TryGetValue(aggregate, out Expression mapped) ? mapped : Visit(aggregate.AggregateAsSubquery);
        }

        private class AggregateGatherer : DbExpressionVisitor
        {
            private readonly List<AggregateSubqueryExpression> aggregates = new List<AggregateSubqueryExpression>();

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
