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
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="S"></typeparam>
    public sealed class ParameterRewriter : Common.Linq.Expressions.ExpressionVisitor
    {
        private readonly object m_obj;
        private readonly ParameterExpression m_parExp;

        /// <summary>
        /// 初始化 <see cref="T:Fireasy.Data.Entity.Linq.Translators.ParameterRewriter`2"/> 类的新实例。
        /// </summary>
        /// <param name="parExp"></param>
        /// <param name="obj"></param>
        private ParameterRewriter(ParameterExpression parExp, object obj)
        {
            m_obj = obj;
            m_parExp = parExp;
        }

        /// <summary>
        /// 将表达式中的指定参数表达式替换为指定的常量进行表示。
        /// </summary>
        /// <param name="expression">要搜寻的表达式。</param>
        /// <param name="parExp">要搜寻的参数表达式。</param>
        /// <param name="obj">替换的常量。</param>
        /// <returns></returns>
        public static Expression Rewrite(Expression expression, ParameterExpression parExp, object obj)
        {
            var lambda = (LambdaExpression)expression;
            return new ParameterRewriter(parExp, obj).Visit(lambda.Body);
        }

        /// <summary>
        /// 访问 <see cref="ParameterExpression"/>。
        /// </summary>
        /// <param name="p">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (m_obj != null && p.Type == m_obj.GetType())
            {
                return Expression.Constant(m_obj);
            }

            if (p.Type == m_parExp.Type)
            {
                return m_parExp;
            }

            return base.VisitParameter(p);
        }
    }
}
