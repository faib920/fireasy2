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
    public class OracleTranslateProvider : TranslateProviderBase
    {
        public override TranslatorBase CreateTranslator(TranslateContext transContext)
        {
            return new OracleTranslator(transContext);
        }

        protected override Expression BuildExpression(Expression expression)
        {
            expression = OrderByRewriter.Rewrite(expression);
            expression = base.BuildExpression(expression);
            expression = SkipToRowNumberRewriter.Rewrite(expression);
            expression = OrderByRewriter.Rewrite(expression);

            return expression;
        }
    }
}
