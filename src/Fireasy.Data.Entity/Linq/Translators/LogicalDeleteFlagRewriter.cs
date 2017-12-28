// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 用于为具有假删除标记的查询表达式添加标记条件。
    /// </summary>
    public class LogicalDeleteFlagRewriter : DbExpressionVisitor
    {
        private ColumnExpression fakeColumn;

        public static Expression Rewrite(Expression expression)
        {
            return new LogicalDeleteFlagRewriter().Visit(expression);
        }

        /// <summary>
        /// 访问 <see cref="SelectExpression"/>。
        /// </summary>
        /// <param name="select">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitSelect(SelectExpression select)
        {
            if (select.From != null && select.From.NodeType == (ExpressionType)DbExpressionType.Table)
            {
                var table = (TableExpression)select.From;
                //首先要找到具有假删除标记的列表达式
                foreach (var column in select.Columns)
                {
                    base.Visit(column.Expression);
                }

                if (fakeColumn != null && fakeColumn.Alias.Equals(table.Alias))
                {
                    var where = select.Where;

                    var condExp = fakeColumn.Equal(Expression.Constant(0.ToType(fakeColumn.Type)));
                    return select.Update(select.From,
                        where != null ? Expression.And(where, condExp) : condExp,
                        select.OrderBy, select.GroupBy, select.Skip, select.Take,
                        select.Segment, select.IsDistinct, select.Columns, select.Having, select.IsReverse);
                }
            }
            else if (select.From != null)
            {
                var from = base.Visit(select.From);
                return select.Update(from, select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                        select.Segment, select.IsDistinct, select.Columns, select.Having, select.IsReverse);
            }

            return select;
        }

        /// <summary>
        /// 访问 <see cref="ColumnExpression"/>。
        /// </summary>
        /// <param name="column">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitColumn(ColumnExpression column)
        {
            //记录下具有假删除标记的列表达式。
            if (fakeColumn == null && column.MapInfo != null && column.MapInfo.IsDeletedKey)
            {
                fakeColumn = column;
            }

            return column;
        }
    }
}
