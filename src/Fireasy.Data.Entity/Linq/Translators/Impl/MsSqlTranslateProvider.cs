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
    public class MsSqlTranslateProvider : TranslateProviderBase
    {
        public override TranslatorBase CreateTranslator()
        {
            return new MsSqlTranslator();
        }

        protected override Expression BuildExpression(Expression expression)
        {
            // fix up any order-by's
            expression = OrderByRewriter.Rewrite(expression);
            expression = base.BuildExpression(expression);
            expression = CrossJoinIsolator.Isolate(expression);
            expression = SkipToRowNumberRewriter.Rewrite(expression);
            expression = OrderByRewriter.Rewrite(expression);
            expression = UnusedColumnRemover.Remove(expression);
            expression = RedundantColumnRemover.Remove(expression);
            return expression;
        }
    }
}
