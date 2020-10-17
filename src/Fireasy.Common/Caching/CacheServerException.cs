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
    /// 缓存服务器相关的异常。
    /// </summary>
    public sealed class CacheServerException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="CacheServerException"/> 类的新实例。
        /// </summary>
        /// <param name="exception">具体的异常信息。</param>
        public CacheServerException(Exception exception)
            : base(SR.GetString(SRKind.CacheServerAnomaly), exception)
        {
        }
    }
}
