// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 表示能够发现 <see cref="IQueryPolicy"/> 实例。
    /// </summary>
    public interface IQueryPolicyAware
    {
        /// <summary>
        /// 获取 <see cref="IQueryPolicy"/> 实例。
        /// </summary>
        IQueryPolicy QueryPolicy { get; }
    }
}
