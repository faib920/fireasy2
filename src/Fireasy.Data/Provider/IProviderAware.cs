// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// 表示能够发现 <see cref="IProvider"/> 实例。
    /// </summary>
    public interface IProviderAware
    {
        /// <summary>
        /// 获取数据库提供者实例。
        /// </summary>
        IProvider Provider { get; }
    }
}
