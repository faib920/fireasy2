// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

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
        public TaskExecuteContext(IServiceProvider serviceProvider, IDictionary<string, object> arguments = null)
        {
            ServiceProvider = serviceProvider;
            Arguments = arguments;
        }

        /// <summary>
        /// 获取应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// 获取执行传递的参数。
        /// </summary>
        public IDictionary<string, object> Arguments { get; private set; }
    }
}
