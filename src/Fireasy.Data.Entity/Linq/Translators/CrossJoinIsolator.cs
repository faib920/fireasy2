// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Isolates cross joins from other types of joins using nested sub queries
    /// </summary>
    public class CrossJoinIsolator : DbExpressionVisitor
    {
        private ILookup<TableAlias, ColumnExpression> columns;
        private readonly Dictionary<ColumnExpression, ColumnExpression> map = new Dictionary<ColumnExpression, ColumnExpression>();
        private JoinType? lastJoin;

        public static Expression Isolate(Expression expression)
        {
            return new CrossJoinIsolator().Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            var saveColumns = columns;
            columns = ReferencedColumnGatherer.Gather(select).ToLookup(c => c.Alias);
            var saveLastJoin = lastJoin;
            lastJoin = null;
            var result = base.VisitSelect(select);
            columns = saveColumns;
            lastJoin = saveLastJoin;
            return result;
        }

        protected override Expression VisitJoin(JoinExpression join)
        {
            var saveLastJoin = lastJoin;
            lastJoin = join.JoinType;
            join = (JoinExpression)base.VisitJoin(join);
            lastJoin = saveLastJoin;

            if (lastJoin != null && (join.JoinType == JoinType.CrossJoin) != (lastJoin == JoinType.CrossJoin))
            {
                var result = MakeSubquery(join);
                return result;
            }

            return join;
        }

        private bool IsCrossJoin(Expression expression)
        {
            if (expression is JoinExpression jex)
            {
                return jex.JoinType == JoinType.CrossJoin;
            }

            return false;
        }

        private Expression MakeSubquery(Expression expression)
        {
            var newAlias = new TableAlias();
            var aliases = DeclaredAliasGatherer.Gather(expression);

            var decls = new List<ColumnDeclaration>();
            foreach (var ta in aliases)
            {
                foreach (var col in columns[ta])
                {
                    var name = decls.GetAvailableColumnName(col.Name);
                    var decl = new ColumnDeclaration(name, col);
                    decls.Add(decl);
                    var newCol = new ColumnExpression(col.Type, newAlias, name, null);
                    map.Add(col, newCol);
                }
            }

            return new SelectExpression(newAlias, decls, expression, null);
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (map.TryGetValue(column, out ColumnExpression mapped))
            {
                return mapped;
            }

            return column;
        }
    }
}