// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data
{
    /// <summary>
    /// 无法获取 <see cref="DatabaseScope"/> 对象时抛出此异常。无法继承此类。
    /// </summary>
    [Serializable]
    [Obsolete]
    public sealed class UnableGetDatabaseScopeException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="UnableGetDatabaseScopeException"/> 类的新实例。
        /// </summary>
        public UnableGetDatabaseScopeException()
            : base (SR.GetString(SRKind.UnableGetDatabaseScope))
        {
        }
    }
}
