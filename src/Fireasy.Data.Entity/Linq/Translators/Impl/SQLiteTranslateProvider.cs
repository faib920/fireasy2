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
    /// SQLite 数据库类型的 ELinq 翻译器提供者。
    /// </summary>
    public class SQLiteTranslateProvider : TranslateProviderBase
    {
        /// <summary>
        /// 获取一个 ELinq 翻译器。
        /// </summary>
        /// <returns></returns>
        public override TranslatorBase CreateTranslator()
        {
            return new SQLiteTranslator();
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
