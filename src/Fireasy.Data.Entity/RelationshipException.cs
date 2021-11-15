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
    /// 实体间关系错误时抛出此异常。无法继承此类。
    /// </summary>
    public sealed class RelationshipException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="RelationshipException"/> 类的新实例。
        /// </summary>
        /// <param name="message">异常的信息。</param>
        /// <param name="innerExp"></param>
        public RelationshipException(string message, Exception innerExp = null)
            : base(message, innerExp)
        {
        }
    }
}
