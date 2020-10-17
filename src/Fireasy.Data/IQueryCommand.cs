// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Data;

namespace Fireasy.Data
{
    /// <summary>
    /// 提供给 <see cref="IDatabase"/> 查询的对象。
    /// </summary>
    public interface IQueryCommand
    {
        /// <summary>
        /// 获取 <see cref="CommandType"/> 类型。
        /// </summary>
        CommandType CommandType { get; }
    }
}
