// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Threading.Tasks;

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 任务执行器。
    /// </summary>
    public interface ITaskExecutor
    {
        /// <summary>
        /// 触发的执行处理。
        /// </summary>
        void Execute(TaskExecuteContext context);
    }

    /// <summary>
    /// 异步的任务执行器。
    /// </summary>
    public interface IAsyncTaskExecutor
    {
        /// <summary>
        /// 触发的执行处理。
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task ExecuteAsync(TaskExecuteContext context);
    }
}
