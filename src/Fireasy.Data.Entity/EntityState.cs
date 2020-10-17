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
    /// 实体的状态。
    /// </summary>
    public enum EntityState
    {
        /// <summary>
        /// 实体未被修改。
        /// </summary>
        Unchanged,

        /// <summary>
        /// 已添加到数据集中，但还未保存到库。
        /// </summary>
        Attached,

        /// <summary>
        /// 已从数据集中移除，但还未从库中删除。
        /// </summary>
        Detached,

        /// <summary>
        /// 该实体的某项属性已被修改。
        /// </summary>
        Modified,
    }
}
