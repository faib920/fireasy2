using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class OleDbTranslateProvider : TranslateProviderBase
    {
        public override TranslatorBase CreateTranslator(TranslateContext transContext)
        {
            return new OleDbTranslator(transContext);
        }

        protected override Expression BuildExpression(Expression expression)
        {
            // fix up any order-by's
            expression = OrderByRewriter.Rewrite(expression);
            expression = base.BuildExpression(expression);
            expression = CrossJoinIsolator.Isolate(expression);
            expression = SkipToNestedOrderByRewriter.Rewrite(expression);
            expression = OrderByRewriter.Rewrite(expression);
            expression = UnusedColumnRemover.Remove(expression);
            //expression = RedundantColumnRemover.Remove(expression);
            return expression;
        }
    }
}
