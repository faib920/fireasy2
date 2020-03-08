// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Splits an expression into two parts
    ///   1) a list of column declarations for sub-expressions that must be evaluated on the server
    ///   2) a expression that describes how to combine/project the columns back together into the correct result
    /// </summary>
    public class ColumnProjector : DbExpressionVisitor
    {
        private readonly Dictionary<ColumnExpression, ColumnExpression> map;
        private readonly List<ColumnDeclaration> columns;
        private readonly HashSet<string> columnNames;
        private readonly HashSet<Expression> candidates;
        private readonly HashSet<TableAlias> existingAliases;
        private readonly TableAlias newAlias;
        private int iColumn;

        private ColumnProjector(Func<Expression, bool> fnCanBeColumn, Expression expression, IEnumerable<ColumnDeclaration> existingColumns, TableAlias newAlias, IEnumerable<TableAlias> existingAliases)
        {
            this.newAlias = newAlias;
            this.existingAliases = new HashSet<TableAlias>(existingAliases);
            map = new Dictionary<ColumnExpression, ColumnExpression>();
            if (existingColumns != null)
            {
                columns = new List<ColumnDeclaration>(existingColumns);
                columnNames = new HashSet<string>(existingColumns.Select(c => c.Name));
            }
            else
            {
                columns = new List<ColumnDeclaration>();
                columnNames = new HashSet<string>();
            }

            candidates = Nominator.Nominate(fnCanBeColumn, expression);
        }

        internal static ProjectedColumns ProjectColumns(Func<Expression, bool> fnCanBeColumn, Expression expression, IEnumerable<ColumnDeclaration> existingColumns, TableAlias newAlias, IEnumerable<TableAlias> existingAliases)
        {
            var projector = new ColumnProjector(fnCanBeColumn, expression, existingColumns, newAlias, existingAliases);
            var expr = projector.Visit(expression);

            return new ProjectedColumns(expr, projector.columns.AsReadOnly());
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

            if (candidates.Contains(expression))
            {
                if (expression.NodeType == (ExpressionType)DbExpressionType.Column)
                {
                    var column = (ColumnExpression)expression;
                    if (map.TryGetValue(column, out ColumnExpression mapped))
                    {
                        return mapped;
                    }

                    // check for column that already refers to this column
                    foreach (var existingColumn in columns)
                    {
                        if (existingColumn.Expression is ColumnExpression cex &&
                            cex.Alias == column.Alias &&
                            cex.Name == column.Name)
                        {
                            // refer to the column already in the column list
                            return new ColumnExpression(column.Type, newAlias, existingColumn.Name, null);
                        }
                    }

                    if (existingAliases.Contains(column.Alias))
                    {
                        var columnName = GetUniqueColumnName(column.Name);
                        columns.Add(new ColumnDeclaration(columnName, column));
                        mapped = new ColumnExpression(column.Type, newAlias, columnName, column.MapInfo);
                        map.Add(column, mapped);
                        columnNames.Add(columnName);
                        return mapped;
                    }

                    // must be referring to outer scope
                    return column;
                }
                else
                {
                    string columnName = GetNextColumnName();
                    columns.Add(new ColumnDeclaration(columnName, expression));
                    return new ColumnExpression(expression.Type, newAlias, columnName, null);
                }
            }
            else
            {
                return base.Visit(expression);
            }
        }

        private bool IsColumnNameInUse(string name)
        {
            return columnNames.Contains(name);
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
            return GetUniqueColumnName("c" + (iColumn++));
        }

        /// <summary>
        /// Nominator is a class that walks an expression tree bottom up, determining the set of 
        /// candidate expressions that are possible columns of a select expression
        /// </summary>
        private class Nominator : DbExpressionVisitor
        {
            private readonly Func<Expression, bool> fnCanBeColumn;
            private bool isBlocked;
            private readonly HashSet<Expression> candidates;

            private Nominator(Func<Expression, bool> fnCanBeColumn)
            {
                this.fnCanBeColumn = fnCanBeColumn;
                candidates = new HashSet<Expression>();
                isBlocked = false;
            }

            internal static HashSet<Expression> Nominate(Func<Expression, bool> fnCanBeColumn, Expression expression)
            {
                var nominator = new Nominator(fnCanBeColumn);
                nominator.Visit(expression);
                return nominator.candidates;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveIsBlocked = this.isBlocked;
                    isBlocked = false;
                    if (fnCanBeColumn(expression))
                    {
                        candidates.Add(expression);
                        // don't merge saveIsBlocked
                    }
                    else
                    {
                        base.Visit(expression);
                        if (!isBlocked)
                        {
                            if (fnCanBeColumn(expression))
                            {
                                candidates.Add(expression);
                            }
                            else
                            {
                                this.isBlocked = true;
                            }
                        }
                        isBlocked |= saveIsBlocked;
                    }
                }
                return expression;
            }

            protected override Expression VisitProjection(ProjectionExpression proj)
            {
                this.Visit(proj.Projector);
                return proj;
            }

        }
    }
}
