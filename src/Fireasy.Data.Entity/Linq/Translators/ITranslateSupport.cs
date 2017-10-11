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
    /// 提供表达式的翻译支持。
    /// </summary>
    public interface ITranslateSupport
    {
        /// <summary>
        /// 执行表达式的翻译。
        /// </summary>
        /// <param name="expression">查询表达式。</param>
        /// <param name="option">指定解析的选项。</param>
        /// <returns>翻译结果对象。</returns>
        TranslateResult Translate(Expression expression, TranslateOptions option = null);
    }
}
