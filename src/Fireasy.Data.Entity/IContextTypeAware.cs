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
    /// 用于感知 <see cref="EntityContext"/> 的类型。
    /// </summary>
    public interface IContextTypeAware
    {
        /// <summary>
        /// 获取 <see cref="EntityContext"/> 的类型。
        /// </summary>
        Type ContextType { get; }
    }
}
