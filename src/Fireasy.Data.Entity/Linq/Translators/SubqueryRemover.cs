// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Removes one or more SelectExpression's by rewriting the expression tree to not include them, promoting
    /// their from clause expressions and rewriting any column expressions that may have referenced them to now
    /// reference the underlying data directly.
    /// </summary>
    public class SubqueryRemover : DbExpressionVisitor
    {
        private readonly HashSet<SelectExpression> _selectsToRemove;
        private readonly Dictionary<TableAlias, Dictionary<string, Expression>> _map;

        private SubqueryRemover(IEnumerable<SelectExpression> selectsToRemove)
        {
            _selectsToRemove = new HashSet<SelectExpression>(selectsToRemove);
            _map = selectsToRemove.ToDictionary(d => d.Alias, d => d.Columns.ToDictionary(d2 => d2.Name, d2 => d2.Expression));
        }

        public static SelectExpression Remove(SelectExpression outerSelect, params SelectExpression[] selectsToRemove)
        {
            return Remove(outerSelect, (IEnumerable<SelectExpression>)selectsToRemove);
        }

        public static SelectExpression Remove(SelectExpression outerSelect, IEnumerable<SelectExpression> selectsToRemove)
        {
            return (SelectExpression)new SubqueryRemover(selectsToRemove).Visit(outerSelect);
        }

        public static ProjectionExpression Remove(ProjectionExpression projection, IEnumerable<SelectExpression> selectsToRemove)
        {
            return (ProjectionExpression)new SubqueryRemover(selectsToRemove).Visit(projection);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            return _selectsToRemove.Contains(select) ? Visit(select.From) : base.VisitSelect(select);
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (_map.TryGetValue(column.Alias, out Dictionary<string, Expression> nameMap))
            {
                if (nameMap.TryGetValue(column.Name, out Expression expr))
                {
                    return Visit(expr);
                }

                throw new Exception(SR.GetString(SRKind.NotDefinedReference));
            }

            return column;
        }
    }
}