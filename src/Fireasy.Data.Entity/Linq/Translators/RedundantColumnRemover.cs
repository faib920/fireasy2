// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Removes duplicate column declarations that refer to the same underlying column
    /// </summary>
    public class RedundantColumnRemover : DbExpressionVisitor
    {
        private readonly Dictionary<ColumnExpression, ColumnExpression> _map = new Dictionary<ColumnExpression, ColumnExpression>();

        public static Expression Remove(Expression expression)
        {
            return new RedundantColumnRemover().Visit(expression);
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            return _map.TryGetValue(column, out ColumnExpression mapped) ? mapped : column;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelect(select);

            // look for redundant column declarations
            var cols = select.Columns.OrderBy(c => c.Name).ToList();
            var removed = new BitArray(select.Columns.Count);

            bool anyRemoved = false;
            for (int i = 0, n = cols.Count; i < n - 1; i++)
            {
                var ci = cols[i];
                var cxi = new ColumnExpression(ci.Expression.Type, select.Alias, ci.Name, ci.Expression is ColumnExpression cix ? cix.MapInfo : null);

                for (int j = i + 1; j < n; j++)
                {
                    if (!removed.Get(j))
                    {
                        var cj = cols[j];
                        if (SameExpression(ci.Expression, cj.Expression))
                        {
                            // any reference to 'j' should now just be a reference to 'i'
                            var cxj = new ColumnExpression(cj.Expression.Type, select.Alias, cj.Name, cj.Expression is ColumnExpression cj1 ? cj1.MapInfo : null);
                            _map.Add(cxj, cxi);
                            removed.Set(j, true);
                            anyRemoved = true;
                        }
                    }
                }
            }

            if (anyRemoved)
            {
                var newDecls = new List<ColumnDeclaration>();
                for (int i = 0, n = cols.Count; i < n; i++)
                {
                    if (!removed.Get(i))
                    {
                        newDecls.Add(cols[i]);
                    }
                }

                select = select.SetColumns(newDecls);
            }

            return select;
        }

        private bool SameExpression(Expression a, Expression b)
        {
            if (a == b)
            {
                return true;
            }


            return (a is ColumnExpression ca && b is ColumnExpression cb && ca.Alias == cb.Alias && ca.Name == cb.Name);
        }
    }
}
