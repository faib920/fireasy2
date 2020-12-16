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
        void ExecuteBatch(IEnumerable<string> commands, ParameterCollection parameters);

        Task ExecuteBatchAsync(IEnumerable<string> commands, ParameterCollection parameters, CancellationToken cancellationToken);
    }
}
