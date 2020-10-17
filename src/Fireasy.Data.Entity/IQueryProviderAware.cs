// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Linq;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 表示能够发现 <see cref="IQueryProvider"/> 实例。
    /// </summary>
    public interface IQueryProviderAware
    {
        /// <summary>
        /// 获取 <see cref="IQueryProvider"/> 实例。
        /// </summary>
        IQueryProvider Provider { get; }
    }
}
