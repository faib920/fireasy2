// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 行数表达式。
    /// </summary>
    public sealed class RowNumberExpression : DbExpression
    {
        public RowNumberExpression(IEnumerable<OrderExpression> orderBy)
            : base(DbExpressionType.RowCount, typeof(int))
        {
            OrderBy = orderBy.ToReadOnly();
        }

        /// <summary>
        /// 获取所包含的排序表达式。
        /// </summary>
        public ReadOnlyCollection<OrderExpression> OrderBy { get; }

        public RowNumberExpression Update(IEnumerable<OrderExpression> orderBy)
        {
            return orderBy != OrderBy ? new RowNumberExpression(orderBy) : this;
        }


    }
}
