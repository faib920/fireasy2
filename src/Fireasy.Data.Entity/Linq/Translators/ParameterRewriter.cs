// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 用于将表达式中的参数表达式替换为常量表示。无法继承此类。
    /// </summary>
    public sealed class ParameterRewriter : Common.Linq.Expressions.ExpressionVisitor
    {
        private readonly object obj;
        private readonly ParameterExpression parExp;

        /// <summary>
        /// 初始化 <see cref="T:Fireasy.Data.Entity.Linq.Translators.ParameterRewriter`2"/> 类的新实例。
        /// </summary>
        /// <param name="parExp"></param>
        /// <param name="obj"></param>
        private ParameterRewriter(ParameterExpression parExp, object obj)
        {
            this.obj = obj;
            this.parExp = parExp;
        }

        /// <summary>
        /// 将表达式中的指定参数表达式替换为指定的常量进行表示。
        /// </summary>
        /// <param name="expression">要搜寻的表达式。</param>
        /// <param name="parExp">要搜寻的参数表达式。</param>
        /// <param name="obj">替换的常量。</param>
        /// <returns></returns>
        public static Expression Rewrite(Expression expression, ParameterExpression parExp, object obj = null)
        {
            return new ParameterRewriter(parExp, obj).Visit(expression);
        }

        /// <summary>
        /// 访问 <see cref="ParameterExpression"/>。
        /// </summary>
        /// <param name="p">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (obj != null && p.Type.IsAssignableFrom(obj.GetType()))
            {
                return Expression.Constant(obj);
            }

            if (p.Type == parExp.Type)
            {
                return parExp;
            }

            return base.VisitParameter(p);
        }
    }
}
