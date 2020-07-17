// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Linq.Expressions;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 基于 DB 的表达式重写器。
    /// </summary>
    public class DbExpressionWriter : ExpressionWriter
    {
        protected DbExpressionWriter(TextWriter writer)
            : base(writer)
        {
        }

        /// <summary>
        /// 将表达式树写入到指定的 <see cref="TextWriter"/> 对象中。
        /// </summary>
        /// <param name="writer">一个 <see cref="TextWriter"/> 对象。</param>
        /// <param name="expression">要重写的表达式。</param>
        public new static void Write(TextWriter writer, Expression expression)
        {
            new DbExpressionWriter(writer).Visit(expression);
        }

        /// <summary>
        /// 将表达式树转换为字符串表示。
        /// </summary>
        /// <param name="expression">要重写的表达式。</param>
        /// <returns>表示表达式的字符串。</returns>
        public new static string WriteToString(Expression expression)
        {
            using var sw = new StringWriter();
            Write(sw, expression);
            return sw.ToString();
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value is IQueryable)
            {
                var queryable = c.Value as IQueryable;
                var elementType = queryable.Expression.Type.GetEnumerableElementType();
                if (typeof(IEntity).IsAssignableFrom(elementType))
                {
                    Write(c.Value.ToString());
                }
                else
                {
                    Visit(queryable.Expression);
                }

                return c;
            }

            return base.VisitConstant(c);
        }
    }
}
