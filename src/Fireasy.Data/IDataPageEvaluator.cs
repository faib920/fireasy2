// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data
{
    /// <summary>
    /// 提供对数据量进行评估的方法。
    /// </summary>
    public interface IDataPageEvaluator
    {
        /// <summary>
        /// 使用上下文对分页对象进行评估。
        /// </summary>
        /// <param name="context">当前的命令上下文对象。</param>
        void Evaluate(CommandContext context);

        /// <summary>
        /// 异步的，使用上下文对分页对象进行评估。
        /// </summary>
        /// <param name="context">当前的命令上下文对象。</param>
        Task EvaluateAsync(CommandContext context, CancellationToken cancellationToken = default);
    }
}
