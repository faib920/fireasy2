// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Query
{
    /// <summary>
    /// 开启一个批处理范围，用于将 Insert、Update、Delete 等操作合并为一条命令。
    /// </summary>
    public sealed class BatchExecuteScope : Scope<BatchExecuteScope>
    {
        private int _parameterIndex = 0;
        private readonly List<string> _commands = new List<string>();
        private readonly ParameterCollection _parameters = new ParameterCollection();

        /// <summary>
        /// 产生一个新的参数名称。
        /// </summary>
        /// <returns></returns>
        public string NewParameterName()
        {
            return "p" + _parameterIndex++;
        }

        /// <summary>
        /// 添加参数。
        /// </summary>
        /// <param name="name">参数名。</param>
        /// <param name="value">参数值。</param>
        public void AddParameter(string name, object value)
        {
            _parameters.Add(name, value);
        }

        /// <summary>
        /// 添加命令。
        /// </summary>
        /// <param name="command"></param>
        public void AddCommand(string command)
        {
            _commands.Add(command);
        }

        /// <summary>
        /// 提交批处理。
        /// </summary>
        /// <param name="context"></param>
        public void Commit(EntityContext context)
        {
            var service = context.GetService<IContextService>();
            if (service is IEntityBatchExecutable executable)
            {
                executable.ExecuteBatch(_commands, _parameters);
            }
            else
            {
                throw new NotSupportedException(SR.GetString(SRKind.NotSupportExecuteBatch));
            }
        }

        /// <summary>
        /// 异步的，提交批处理。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CommitAsync(EntityContext context, CancellationToken cancellationToken = default)
        {
            var service = context.GetService<IContextService>();
            if (service is IEntityBatchExecutable executable)
            {
                await executable.ExecuteBatchAsync(_commands, _parameters, cancellationToken);
            }
            else
            {
                throw new NotSupportedException(SR.GetString(SRKind.NotSupportExecuteBatch));
            }
        }
    }
}
