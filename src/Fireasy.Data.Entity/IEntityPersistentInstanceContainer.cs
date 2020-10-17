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
    /// 用于放置当前环境中有效的实例名称。
    /// </summary>
    public interface IEntityPersistentInstanceContainer
    {
        /// <summary>
        /// 获取或设置实例名称。
        /// </summary>
        string InstanceName { get; set; }
    }
}
