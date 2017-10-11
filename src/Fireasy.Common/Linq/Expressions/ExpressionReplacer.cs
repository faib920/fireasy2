using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Fireasy.Common.Linq.Expressions
{
    public class ExpressionReplacer : ExpressionVisitor
    {
        private ICollection<ParameterExpression> pars;

        public ExpressionReplacer(ICollection<ParameterExpression> pars)
        {
            this.pars = pars;
        }

        public static Expression Replace(Expression expression, ICollection<ParameterExpression> pars)
        {
            var instance = new ExpressionReplacer(pars);
            return instance.Visit(expression);
        }

        public static Expression Replace(Expression expression, params ParameterExpression[] pars)
        {
            var instance = new ExpressionReplacer(pars);
            return instance.Visit(expression);
        }

        protected override Expression VisitMember(MemberExpression memberExp)
        {
            var parExp = memberExp.Expression as ParameterExpression;
            if (parExp != null)
            {
                var p = pars.FirstOrDefault(s => s.Type == parExp.Type);

                if (p != null)
                {
                    return Expression.MakeMemberAccess(p, memberExp.Member);
                }
            }

            return memberExp;
        }

    }
}
