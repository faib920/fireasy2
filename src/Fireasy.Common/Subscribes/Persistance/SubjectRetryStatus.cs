// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.Subscribes.Persistance
{
    /// <summary>
    /// 主题重试的返回状态。
    /// </summary>
    public enum SubjectRetryStatus
    {
        /// <summary>
        /// 发布成功。
        /// </summary>
        Success,
        /// <summary>
        /// 发布失败。
        /// </summary>
        Failed,
        /// <summary>
        /// 达到次数。
        /// </summary>
        OutOfTimes
    }
}
