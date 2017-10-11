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
    public class MySqlTranslateProvider : TranslateProviderBase
    {
        public override TranslatorBase CreateTranslator()
        {
            return new MySqlTranslator();
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
