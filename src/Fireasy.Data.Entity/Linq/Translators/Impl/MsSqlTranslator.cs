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
    public class MsSqlTranslator : TranslatorBase
    {
        public MsSqlTranslator(TranslateContext transContext)
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

            if (select.Take != null && select.Skip == null)
            {
                Write("TOP ");
                Visit(select.Take);
                Write(" ");
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

            if (select.OrderBy != null && select.OrderBy.Count > 0)
            {
                WriteOrders(select);
            }

            base.Visit(select.Segment);

            return select;
        }

        protected override Expression VisitRowNumber(RowNumberExpression rowNumber)
        {
            Write("ROW_NUMBER() OVER(");
            if (rowNumber.OrderBy != null && rowNumber.OrderBy.Count > 0)
            {
                Write("ORDER BY ");
                for (int i = 0, n = rowNumber.OrderBy.Count; i < n; i++)
                {
                    var exp = rowNumber.OrderBy[i];
                    if (i > 0)
                    {
                        Write(", ");
                    }

                    VisitValue(exp.Expression);

                    if (exp.OrderType != OrderType.Ascending)
                    {
                        Write(" DESC");
                    }
                }
            }
            else
            {
                Write("ORDER BY (SELECT 1)");
            }

            Write(")");
            return rowNumber;
        }
    }
}
