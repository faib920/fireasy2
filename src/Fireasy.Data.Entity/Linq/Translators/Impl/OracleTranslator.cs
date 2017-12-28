// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq.Expressions;
using System.Linq.Expressions;
using Fireasy.Data.Provider;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class OracleTranslator : TranslatorBase
    {
        protected override Expression VisitSelect(SelectExpression select)
        {
            if (Options.WhereOnly)
            {
                VisitPredicate(select.Where);
                return select;
            }

            if (select.Take != null)
            {
                Write("SELECT * FROM (");
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
            else
            {
                WriteLine(Indentation.Same);
                Write("FROM DUAL");
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

            if (select.Take != null)
            {
                Write(") WHERE ROWNUM<=");
                Write(select.Take);
            }
            else
            {
                base.Visit(select.Segment);
            }

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
                    OrderExpression exp = rowNumber.OrderBy[i];
                    if (i > 0)
                        Write(", ");
                    this.VisitValue(exp.Expression);
                    if (exp.OrderType != OrderType.Ascending)
                        Write(" DESC");
                }
            }
            Write(")");
            return rowNumber;
        }

        protected override Expression VisitStringMethod(MethodCallExpression m)
        {
            if (m.Method.Name == "IsNullOrEmpty")
            {
                Write("(");
                Visit(m.Arguments[0]);
                Write(" IS NULL)");
                return m;
            }

            return base.VisitStringMethod(m);
        }

        protected override void WriteAs()
        {
            Write(" ");
        }

        protected override Expression VisitAggregate(AggregateExpression aggregate)
        {
            if (aggregate.AggregateType == AggregateType.Average)
            {
                Write("TRUNC(");
                var exp = base.VisitAggregate(aggregate);
                Write(", 20)");
                return exp;
            }

            return base.VisitAggregate(aggregate);
        }
    }
}
