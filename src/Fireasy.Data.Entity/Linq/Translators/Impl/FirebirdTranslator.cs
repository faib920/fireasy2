// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq.Expressions;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class FirebirdTranslator : TranslatorBase
    {
        public FirebirdTranslator(TranslateContext transContext)
            : base(transContext)
        {
        }

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

            if (select.Having != null)
            {
                WriteLine(Indentation.Same);
                Write("HAVING ");
                Visit(select.Having);
            }

            if (select.OrderBy != null)
            {
                WriteOrders(select);
            }

            if (select.Take != null && select.Skip != null)
            {
                WriteLine(Indentation.Same);
                Write(" ROWS ");
                Visit(select.Skip);
                WriteLine(Indentation.Same);
                Write(" TO ");
                Visit(select.Skip);
                Write(" + ");
                Visit(select.Take);
                Write(" - 1");
            }
            else if (select.Take != null)
            {
                Write(" ROWS ");
                Visit(select.Take);
            }

            base.Visit(select.Segment);

            return select;
        }
    }
}
