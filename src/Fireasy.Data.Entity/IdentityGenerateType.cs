// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Identity;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 标识生成类型。
    /// </summary>
    public enum IdentityGenerateType
    {
        /// <summary>
        /// 无生成，手动指定。
        /// </summary>
        None,

        /// <summary>
        /// 使用自增长支持。如果不受支持则尝试使用 Generator 来生成标识。
        /// </summary>
        AutoIncrement,

        /// <summary>
        /// 使用 <see cref="IGeneratorProvider"/> 生成器生成。
        /// </summary>
        Generator
    }
}
