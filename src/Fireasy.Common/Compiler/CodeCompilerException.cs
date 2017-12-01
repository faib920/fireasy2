// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETSTANDARD2_0
using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace Fireasy.Common.Compiler
{
    /// <summary>
    /// 动态编译代码期间发生的异常。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class CodeCompilerException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="CodeCompilerException"/> 类的新实例。
        /// </summary>
        /// <param name="message">编译错误信息。</param>
        /// <param name="errors">编译器返回的错误集合。</param>
        public CodeCompilerException(string message, CompilerErrorCollection errors)
            : base (message)
        {
            Errors = errors;
        }

        private CodeCompilerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Errors = (CompilerErrorCollection)info.GetValue("Errors", typeof(CompilerErrorCollection));
        }

        /// <summary>
        /// 获取编译器返回的错误集合。
        /// </summary>
        public CompilerErrorCollection Errors { get; private set; }

        /// <summary>
        /// 设置异常信息。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Errors", Errors);
        }
    }
}
#endif