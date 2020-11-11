// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 用于分组时将 Where 转换为 Having。
    /// </summary>
    public class HavingRewriter : DbExpressionVisitor
    {
        private readonly List<ColumnDeclaration> _aggColumns = new List<ColumnDeclaration>();
        private Expression _having;
        private List<Expression> _havings = new List<Expression>();
        private bool _isAdd;
        private bool _hasGroupby;

        public static Expression Rewriter(Expression expression)
        {
            return new HavingRewriter().Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            if (!_hasGroupby)
            {
                _hasGroupby = select.GroupBy.Count > 0;
            }

            var from = Visit(select.From);

            if (!_hasGroupby)
            {
                return select;
            }

            foreach (var cd in select.Columns)
            {
                if (cd.Expression is AggregateExpression)
                {
                    _aggColumns.Add(cd);
                }
            }

            var where = Visit(select.Where);

            if (from is SelectExpression select1 && _having != null)
            {
                from = select1.Update(select1.From, null, select1.OrderBy, select1.GroupBy, select1.Skip, select1.Take,
                    select1.Segment, select1.IsDistinct, select1.Columns, _having, select1.IsReverse);

                return select.Update(from, where, select.OrderBy, select.GroupBy, select.Skip,
                    select.Take, select.Segment, select.IsDistinct, select.Columns, null, select.IsReverse);
            }

            return select;
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            var select = (SelectExpression)Visit(proj.Select);
            return proj.Update(select, proj.Projector, proj.Aggregator);
        }

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            _isAdd = false;
            var left = Visit(binary.Left);
            var right = Visit(binary.Right);

            if (_isAdd && left != null && right != null)
            {
                _having = Expression.MakeBinary(binary.NodeType, left, right);
                _havings.Add(_having);

                if (left != binary.Left)
                {
                    return null;
                }

                if (right != binary.Right)
                {
                    return null;
                }
            }

            if (_havings.Count == 2)
            {
                _having = Expression.MakeBinary(binary.NodeType, _havings[0], _havings[1]);
                _havings.Clear();
                _havings.Add(_having);
            }

            if (left == null) return right;
            if (right == null) return left;

            return binary;
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            var cd = _aggColumns.FirstOrDefault(s => s.Name == column.Name);
            if (cd != null)
            {
                _isAdd = true;
                return cd.Expression;
            }

            return column;
        }
    }
}
