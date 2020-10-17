using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Common.Linq.Expressions
{
    public class ExpressionReplacer : ExpressionVisitor
    {
        private readonly ICollection<ParameterExpression> _pars;

        public ExpressionReplacer(ICollection<ParameterExpression> pars)
        {
            _pars = pars;
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
                var p = _pars.FirstOrDefault(s => s.Type == parExp.Type);

                if (p != null)
                {
                    return Expression.MakeMemberAccess(p, memberExp.Member);
                }
            }

            var exp = Visit(memberExp.Expression);
            return Expression.MakeMemberAccess(exp, memberExp.Member);
        }
    }
}
