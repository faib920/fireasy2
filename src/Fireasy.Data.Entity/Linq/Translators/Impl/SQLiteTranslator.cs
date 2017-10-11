// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Provider;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// SQLite 数据库类型的 ELinq 翻译器。
    /// </summary>
    public class SQLiteTranslator : TranslatorBase
    {
        /// <summary>
        /// 访问 <see cref="SelectExpression"/>。
        /// </summary>
        /// <param name="select">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitSelect(SelectExpression select)
        {
            if (Options.WhereOnly)
            {
                VisitPredicate(select.Where);
                return select;
            }

            Write("SELECT ");

            if (select.IsDistinct)
            {
                Write("DISTINCT ");
            }

            WriteColumns(select.Columns);

            if (select.From != null)
            {
                WriteLine(Indentation.Same);
                Write("FROM ");
                VisitSource(select.From);
            }

            if (select.Where != null)
            {
                WriteLine(Indentation.Same);
                Write("WHERE ");
                VisitPredicate(select.Where);
            }

            if (select.GroupBy != null)
            {
                WriteGroups(select);
            }

            if (select.OrderBy != null)
            {
                WriteOrders(select);
            }

            if (select.Take != null)
            {
                WriteLine(Indentation.Same);
                Write("LIMIT ");
                Visit(select.Take);
            }
            else if (select.Skip != null)
            {
                WriteLine(Indentation.Same);
                Write("LIMIT 1000");
            }

            if (select.Skip != null)
            {
                Write(" OFFSET ");
                Visit(select.Skip);
            }

            base.Visit(select.Segment);

            return select;
        }
    }
}
