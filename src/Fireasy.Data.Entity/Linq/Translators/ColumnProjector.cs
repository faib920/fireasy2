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
    /// Splits an expression into two parts
    ///   1) a list of column declarations for sub-expressions that must be evaluated on the server
    ///   2) a expression that describes how to combine/project the columns back together into the correct result
    /// </summary>
    public class ColumnProjector : DbExpressionVisitor
    {
        private readonly Dictionary<ColumnExpression, ColumnExpression> _map;
        private readonly List<ColumnDeclaration> _columns;
        private readonly HashSet<string> _columnNames;
        private readonly HashSet<Expression> _candidates;
        private readonly HashSet<TableAlias> _existingAliases;
        private readonly TableAlias _newAlias;
        private int _column;

        private ColumnProjector(Func<Expression, bool> fnCanBeColumn, Expression expression, IEnumerable<ColumnDeclaration> existingColumns, TableAlias newAlias, IEnumerable<TableAlias> existingAliases)
        {
            _newAlias = newAlias;
            _existingAliases = new HashSet<TableAlias>(existingAliases);
            _map = new Dictionary<ColumnExpression, ColumnExpression>();
            if (existingColumns != null)
            {
                _columns = new List<ColumnDeclaration>(existingColumns);
                _columnNames = new HashSet<string>(existingColumns.Select(c => c.Name));
            }
            else
            {
                _columns = new List<ColumnDeclaration>();
                _columnNames = new HashSet<string>();
            }

            _candidates = Nominator.Nominate(fnCanBeColumn, expression);
        }

        internal static ProjectedColumns ProjectColumns(Func<Expression, bool> fnCanBeColumn, Expression expression, IEnumerable<ColumnDeclaration> existingColumns, TableAlias newAlias, IEnumerable<TableAlias> existingAliases)
        {
            var projector = new ColumnProjector(fnCanBeColumn, expression, existingColumns, newAlias, existingAliases);
            var expr = projector.Visit(expression);

            return new ProjectedColumns(expr, projector._columns.AsReadOnly());
        }

        internal static ProjectedColumns ProjectColumns(Func<Expression, bool> fnCanBeColumn, Expression expression, IEnumerable<ColumnDeclaration> existingColumns, TableAlias newAlias, params TableAlias[] existingAliases)
        {
            return ProjectColumns(fnCanBeColumn, expression, existingColumns, newAlias, (IEnumerable<TableAlias>)existingAliases);
        }

        public override Expression Visit(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (_candidates.Contains(expression))
            {
                if (expression.NodeType == (ExpressionType)DbExpressionType.Column)
                {
                    var column = (ColumnExpression)expression;
                    if (_map.TryGetValue(column, out ColumnExpression mapped))
                    {
                        return mapped;
                    }

                    // check for column that already refers to this column
                    foreach (var existingColumn in _columns)
                    {
                        if (existingColumn.Expression is ColumnExpression cex &&
                            cex.Alias == column.Alias &&
                            cex.Name == column.Name)
                        {
                            // refer to the column already in the column list
                            return new ColumnExpression(column.Type, _newAlias, existingColumn.Name, null);
                        }
                    }

                    if (_existingAliases.Contains(column.Alias))
                    {
                        var columnName = GetUniqueColumnName(column.Name);
                        _columns.Add(new ColumnDeclaration(columnName, column));
                        mapped = new ColumnExpression(column.Type, _newAlias, columnName, column.MapInfo);
                        _map.Add(column, mapped);
                        _columnNames.Add(columnName);
                        return mapped;
                    }

                    // must be referring to outer scope
                    return column;
                }
                else
                {
                    string columnName = GetNextColumnName();
                    _columns.Add(new ColumnDeclaration(columnName, expression));
                    return new ColumnExpression(expression.Type, _newAlias, columnName, null);
                }
            }
            else
            {
                return base.Visit(expression);
            }
        }

        private bool IsColumnNameInUse(string name)
        {
            return _columnNames.Contains(name);
        }

        private string GetUniqueColumnName(string name)
        {
            var baseName = name;
            var suffix = 1;
            while (IsColumnNameInUse(name))
            {
                name = baseName + (suffix++);
            }

            return name;
        }

        private string GetNextColumnName()
        {
            return GetUniqueColumnName("c" + (_column++));
        }

        /// <summary>
        /// Nominator is a class that walks an expression tree bottom up, determining the set of 
        /// candidate expressions that are possible columns of a select expression
        /// </summary>
        private class Nominator : DbExpressionVisitor
        {
            private readonly Func<Expression, bool> _fnCanBeColumn;
            private bool _isBlocked;
            private readonly HashSet<Expression> _candidates;

            private Nominator(Func<Expression, bool> fnCanBeColumn)
            {
                _fnCanBeColumn = fnCanBeColumn;
                _candidates = new HashSet<Expression>();
                _isBlocked = false;
            }

            internal static HashSet<Expression> Nominate(Func<Expression, bool> fnCanBeColumn, Expression expression)
            {
                var nominator = new Nominator(fnCanBeColumn);
                nominator.Visit(expression);
                return nominator._candidates;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveIsBlocked = _isBlocked;
                    _isBlocked = false;
                    if (_fnCanBeColumn(expression))
                    {
                        _candidates.Add(expression);
                        // don't merge saveIsBlocked
                    }
                    else
                    {
                        base.Visit(expression);
                        if (!_isBlocked)
                        {
                            if (_fnCanBeColumn(expression))
                            {
                                _candidates.Add(expression);
                            }
                            else
                            {
                                _isBlocked = true;
                            }
                        }
                        _isBlocked |= saveIsBlocked;
                    }
                }
                return expression;
            }

            protected override Expression VisitProjection(ProjectionExpression proj)
            {
                Visit(proj.Projector);
                return proj;
            }

        }
    }
}
