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
    /// 批处理操作的选项。
    /// </summary>
    public class BatchOperateOptions
    {
        /// <summary>
        /// 获取或设置属性的检查方式。
        /// </summary>
        public BatchCheckModifiedKinds CheckModifiedKinds { get; set; }
    }

    /// <summary>
    /// 批处理时如何检查实体变更的属性。
    /// </summary>
    public enum BatchCheckModifiedKinds
    {
        /// <summary>
        /// 以第一个实体的变更的属性为基准（默认）。
        /// </summary>
        First,
        /// <summary>
        /// 遍列所有实体，取所有变更的属性的交集。
        /// </summary>
        Everyone
    }
}
