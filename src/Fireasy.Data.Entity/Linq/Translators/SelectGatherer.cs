// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// returns the list of SelectExpressions accessible from the source expression
    /// </summary>
    public class SelectGatherer : DbExpressionVisitor
    {
        private readonly List<SelectExpression> selects = new List<SelectExpression>();

        private SelectGatherer()
        {
            selects = new List<SelectExpression>();
        }

        public static ReadOnlyCollection<SelectExpression> Gather(Expression expression)
        {
            var gatherer = new SelectGatherer();
            gatherer.Visit(expression);
            return gatherer.selects.AsReadOnly();
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            selects.Add(select);
            return select;
        }
    }
}
