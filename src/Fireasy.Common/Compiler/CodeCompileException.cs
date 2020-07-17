// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Compiler
{
    /// <summary>
    /// 动态编译代码期间发生的异常。无法继承此类。
    /// </summary>
    public sealed class CodeCompileException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="CodeCompileException"/> 类的新实例。
        /// </summary>
        /// <param name="message">编译错误信息。</param>
        public CodeCompileException(string message)
            : base(message)
        {
        }
    }
}