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
    /// 实体节点的参照位置。
    /// </summary>
    public enum EntityTreePosition
    {
        /// <summary>
        /// 参考实体之前。
        /// </summary>
        Before,

        /// <summary>
        /// 参考实体之后。
        /// </summary>
        After,

        /// <summary>
        /// 参考实体的孩子。
        /// </summary>
        Children
    }
}
