// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Linq.Expressions;
using System;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// ELinq 表示表达式翻译时发生的异常。无法继承此类。
    /// </summary>
    public sealed class TranslateException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="TranslateException"/> 类的新实例。
        /// </summary>
        /// <param name="expression">正在翻译的 ELinq 表达式。</param>
        /// <param name="exception">内部异常。</param>
        public TranslateException(Expression expression, Exception exception)
            : base(SR.GetString(SRKind.FailInTranslateExpression) + exception?.Message + Environment.NewLine + ExpressionWriter.WriteToString(expression), exception)
        {
            Expression = expression;
        }

        /// <summary>
        /// 获取正在翻译的表达式。
        /// </summary>
        public Expression Expression
        {
            get;
            private set;
        }
    }
}
