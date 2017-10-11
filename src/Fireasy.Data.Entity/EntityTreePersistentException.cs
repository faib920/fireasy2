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
    /// 实体树持久化时引发的异常。
    /// </summary>
    public class EntityTreePersistentException : EntityPersistentException
    {
        /// <summary>
        /// 初始化 <see cref="EntityTreePersistentException"/> 类的新实例。
        /// </summary>
        /// <param name="message">指定此异常的信息。</param>
        /// <param name="exception">内联的 <see cref="Exception"/> 信息。</param>
        public EntityTreePersistentException(string message, Exception exception)
            : base (message, exception)
        {
        }
    }
}
