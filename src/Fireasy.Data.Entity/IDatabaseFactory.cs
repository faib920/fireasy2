// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 用于创建 <see cref="IDatabase"/> 实例的工厂。
    /// </summary>
    public interface IDatabaseFactory
    {
        /// <summary>
        /// 创建一个 <see cref="IDatabase"/> 实例。
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IDatabase Create(EntityContextOptions options);
    }
}
