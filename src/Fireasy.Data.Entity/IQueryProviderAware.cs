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
    /// 表示能够发现 <see cref="IQueryProvider"/> 对象。
    /// </summary>
    public interface IQueryProviderAware
    {
        IQueryProvider Provider { get; }
    }
}
