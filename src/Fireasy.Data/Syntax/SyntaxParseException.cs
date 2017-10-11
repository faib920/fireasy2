// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Syntax
{
    /// <summary>
    /// 语法解析时发生的异常。
    /// </summary>
    [Serializable]
    public sealed class SyntaxParseException : Exception
    {
        /// <summary>
        /// 使用语句法初始化 <see cref="SyntaxParseException"/> 类的新实例。
        /// </summary>
        /// <param name="syntaxName">语法名称。</param>
        public SyntaxParseException(string syntaxName)
            : base (SR.GetString(SRKind.SyntaxParseNotSupported, syntaxName))
        {
        }
    }
}
