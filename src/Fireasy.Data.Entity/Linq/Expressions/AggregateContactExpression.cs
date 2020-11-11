// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    public class AggregateContactExpression : DbExpression
    {
        public AggregateContactExpression(Expression separator, ColumnExpression column)
            : base (DbExpressionType.AggregateContact, typeof(string))
        {
            Separator = separator;
            Column = column;
        }

        /// <summary>
        /// 获取分隔符表达式。
        /// </summary>
        public Expression Separator { get; }

        /// <summary>
        /// 获取所包含的数据列表达式。
        /// </summary>
        public ColumnExpression Column { get; }

        /// <summary>
        /// 更新 <see cref="AggregateContactExpression"/> 对象。
        /// </summary>
        /// <param name="column">所包含的数据列表达式。</param>
        /// <returns></returns>
        public AggregateContactExpression Update(ColumnExpression column)
        {
            if (column != Column)
            {
                return new AggregateContactExpression(Separator, column);
            }

            return this;
        }

        public override string ToString()
        {
            return $"AggregateContact({Column})";
        }
    }
}
