// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 任务运行器。
    /// </summary>
    public interface ITaskRunner
    {
        /// <summary>
        /// 开始运行。
        /// </summary>
        void Start();

        /// <summary>
        /// 停止运行。
        /// </summary>
        void Stop();
    }
}
