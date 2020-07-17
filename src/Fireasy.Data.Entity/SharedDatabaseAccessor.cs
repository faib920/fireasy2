// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 用于在同一线程内共享 <see cref="IDatabase"/> 实例。
    /// </summary>
    public sealed class SharedDatabaseAccessor : DisposeableBase
    {
        public IDatabase Database { get; set; }
    }
}