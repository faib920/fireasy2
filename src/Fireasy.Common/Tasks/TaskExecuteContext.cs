// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading;

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 任务执行的上下文对象。
    /// </summary>
    public class TaskExecuteContext
    {
        /// <summary>
        /// 初始化 <see cref="TaskExecuteContext"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider">应用程序服务提供者实例。</param>
        /// <param name="arguments">执行传递的参数。</param>
        /// <param name="cancellationToken"></param>
        public TaskExecuteContext(IServiceProvider serviceProvider, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            ServiceProvider = serviceProvider;
            Arguments = arguments;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// 获取应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// 获取执行传递的参数。
        /// </summary>
        public IDictionary<string, object> Arguments { get; private set; }

        /// <summary>
        /// 获取取消灵牌。
        /// </summary>
        public CancellationToken CancellationToken { get; private set; }
    }
}
