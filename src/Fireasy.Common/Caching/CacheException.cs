// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 缓存操作中发生的异常。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class CacheException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="CacheException"/> 类的新实例。
        /// </summary>
        /// <param name="message">自定义异常信息。</param>
        /// <param name="innerException">内部异常。</param>
        public CacheException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
