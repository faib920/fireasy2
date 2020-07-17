using Fireasy.Data.Entity.Linq.Expressions;
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class AccessTranslator : TranslatorBase
    {
        public AccessTranslator(TranslateContext transContext)
            : base(transContext)
        {
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            if (select.Skip != null)
            {
                if (select.OrderBy == null && select.OrderBy.Count == 0)
                {
                    throw new NotSupportedException("Access cannot support the 'skip' operation without explicit ordering");
                }
                else if (select.Take == null)
                {
                    throw new NotSupportedException("Access cannot support the 'skip' operation without the 'take' operation");
                }
                else
                {
                    throw new NotSupportedException("Access cannot support the 'skip' operation in this query");
                }
            }

            return base.VisitSelect(select);
        }

        protected override Expression VisitJoin(JoinExpression join)
        {
            if (join.JoinType == JoinType.CrossJoin)
            {
                VisitSource(join.Left);
                Write(", ");
                VisitSource(join.Right);
                return join;
            }

            return base.VisitJoin(join);
        }

        protected override void WriteColumns(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            if (columns.Count == 0)
            {
                Write("0");
            }
            else
            {
                base.WriteColumns(columns);
            }
        }

        protected override Expression VisitCompareMethod(MethodCallExpression m)
        {
            if (!m.Method.IsStatic && m.Arguments.Count == 1)
            {
                Write("IIF(");
                Visit(m.Object);
                Write(" = ");
                Visit(m.Arguments[0]);
                Write(", 0, IIF(");
                Visit(m.Object);
                Write(" < ");
                Visit(m.Arguments[0]);
                Write(", -1 ,1))");
            }
            else if (m.Method.IsStatic && m.Arguments.Count == 2)
            {
                Write("IIF(");
                Visit(m.Arguments[0]);
                Write(" = ");
                Visit(m.Arguments[1]);
                Write(", 0, IIF(");
                Visit(m.Arguments[0]);
                Write(" < ");
                Visit(m.Arguments[1]);
                Write(", -1 ,1))");
            }

            return m;
        }

        protected override Expression VisitConditional(ConditionalExpression c)
        {
            Write("IIF(");
            VisitPredicate(c.Test);
            Write(", ");
            VisitValue(c.IfTrue);
            Write(", ");
            VisitValue(c.IfFalse);
            Write(")");
            return c;
        }

        protected override Expression VisitValue(Expression expr)
        {
            if (IsPredicate(expr))
            {
                Write("IIF(");
                Visit(expr);
                Write(", 1, 0)");
            }
            else
            {
                Visit(expr);
            }
            return expr;
        }
    }
}
