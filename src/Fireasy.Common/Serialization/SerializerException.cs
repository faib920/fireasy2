// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 序列化或反序列化时抛出的异常。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class SerializationException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="SerializationException"/> 类的新实例。
        /// </summary>
        /// <param name="message">异常信息。</param>
        /// <param name="innerExp">内部异常信息。</param>
        public SerializationException(string message, Exception innerExp = null)
            : base(message, innerExp)
        {
        }
    }
}
