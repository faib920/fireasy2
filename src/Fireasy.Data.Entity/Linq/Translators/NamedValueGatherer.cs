// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class NamedValueGatherer : DbExpressionVisitor
    {
        private readonly HashSet<NamedValueExpression> _namedValues = new HashSet<NamedValueExpression>(new NamedValueComparer());

        private NamedValueGatherer()
        {
        }

        public static ReadOnlyCollection<NamedValueExpression> Gather(Expression expr)
        {
            var gatherer = new NamedValueGatherer();
            gatherer.Visit(expr);
            return gatherer._namedValues.ToList().AsReadOnly();
        }

        protected override Expression VisitNamedValue(NamedValueExpression value)
        {
            _namedValues.Add(value);
            return value;
        }

        protected override Expression VisitSqlText(SqlExpression sql)
        {
            if (sql.Parameters != null)
            {
                foreach (var p in sql.Parameters)
                {
                    if (!_namedValues.Contains(p))
                    {
                        _namedValues.Add(p);
                    }
                }
            }
            return sql;
        }
    }

    class NamedValueComparer : IEqualityComparer<NamedValueExpression>
    {
        public bool Equals(NamedValueExpression x, NamedValueExpression y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(NamedValueExpression obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}