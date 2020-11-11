// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Data.Provider;

namespace Fireasy.Data.Identity
{
    /// <summary>
    /// 提供列自动生成的方法。
    /// </summary>
    public interface IGeneratorProvider : IProviderService
    {
        /// <summary>
        /// 自动生成列的值。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="tableName">表的名称。</param>
        /// <param name="columnName">列的名称。</param>
        /// <returns>用于标识唯一性的值。</returns>
        long GenerateValue(IDatabase database, string tableName, string columnName = null);
    }
}
