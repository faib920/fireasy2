// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data
{
    /// <summary>
    /// 提供数据分段的方法。
    /// </summary>
    public interface IDataSegment
    {
        /// <summary>
        /// 获取或设置记录开始的位置。
        /// </summary>
        int? Start { get; set; }

        /// <summary>
        /// 获取或设置记录终止的位置。
        /// </summary>
        int? End { get; set; }

        /// <summary>
        /// 获取数据的长度。
        /// </summary>
        int Length { get; }
    }
}
