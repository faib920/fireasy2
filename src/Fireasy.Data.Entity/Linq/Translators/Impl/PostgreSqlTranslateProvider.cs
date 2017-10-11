using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class PostgreSqlTranslateProvider : TranslateProviderBase
    {
        public override TranslatorBase CreateTranslator()
        {
            return new PostgreSqlTranslator();
        }

        protected override Expression BuildExpression(Expression expression)
        {
            expression = OrderByRewriter.Rewrite(expression);
            expression = base.BuildExpression(expression);
            expression = UnusedColumnRemover.Remove(expression);
            return expression;
        }

    }
}
