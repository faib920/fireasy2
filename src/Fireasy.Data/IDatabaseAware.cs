// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Data
{
    /// <summary>
    /// 表示能够发现 <see cref="IDatabase"/> 实例。
    /// </summary>
    public interface IDatabaseAware
    {
        /// <summary>
        /// 获取 <see cref="IDatabase"/> 实例。
        /// </summary>
        IDatabase Database { get; }
    }
}
