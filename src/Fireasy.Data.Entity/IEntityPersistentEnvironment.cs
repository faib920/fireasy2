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
    /// 提供持久化环境的支持。
    /// </summary>
    public interface IEntityPersistentEnvironment
    {
        /// <summary>
        /// 获取或设置环境对象。
        /// </summary>
        EntityPersistentEnvironment Environment { get; set; }
    }
}
