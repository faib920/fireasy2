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
    /// 实体树更新的动作类型。
    /// </summary>
    public enum EntityTreeUpdatingAction
    {
        /// <summary>
        /// 当前节点移动到目标节点的相应位置。
        /// </summary>
        Move,
        /// <summary>
        /// 当前节点被移除。
        /// </summary>
        Remove,
        /// <summary>
        /// 当前节点的名称被改变。
        /// </summary>
        Rename
    }
}