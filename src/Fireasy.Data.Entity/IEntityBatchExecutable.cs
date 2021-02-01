// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 定义对处理的支持。
    /// </summary>
    public interface IEntityBatchExecutable
    {
        /// <summary>
        /// 处理批处理命令。
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="parameters"></param>
        void ExecuteBatch(IEnumerable<string> commands, ParameterCollection parameters);

        /// <summary>
        /// 异步的，处理批处理命令。
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ExecuteBatchAsync(IEnumerable<string> commands, ParameterCollection parameters, CancellationToken cancellationToken);
    }
}
