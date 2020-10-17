// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Data.Provider;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 提供对 ELinq 表达式的翻译。
    /// </summary>
    public interface ITranslateProvider : IProviderService
    {
        /// <summary>
        /// 创建一个翻译器对象。
        /// </summary>
        /// <param name="transContext"></param>
        /// <returns></returns>
        TranslatorBase CreateTranslator(TranslateContext transContext);

        /// <summary>
        /// 对 ELinq 表达式进行翻译，并返回翻译的结果。
        /// </summary>
        /// <param name="transContext"></param>
        /// <param name="expression">一个 ELinq 表达式。</param>
        /// <returns></returns>
        Expression Translate(TranslateContext transContext, Expression expression);

        /// <summary>
        /// 判断表达式中的常量是否可以被计算。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        bool CanBeEvaluatedLocally(Expression expression);
    }
}
