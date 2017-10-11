// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实例持久化时引发的异常。
    /// </summary>
    public class EntityPersistentException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="EntityPersistentException"/> 类的新实例。
        /// </summary>
        /// <param name="message">指定此异常的信息。</param>
        /// <param name="exception">内联的 <see cref="Exception"/> 信息。</param>
        public EntityPersistentException(string message, Exception exception)
            : base (message, exception)
        {
        }
    }
}
