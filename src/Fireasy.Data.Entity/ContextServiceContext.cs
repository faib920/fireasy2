// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// <see cref=" IContextService"/> 初始化的上下文对象。
    /// </summary>
    public sealed class ContextServiceContext
    {
        /// <summary>
        /// 初始化 <see cref="ContextServiceContext"/> 类的新实例。
        /// </summary>
        /// <param name="identification"></param>
        public ContextServiceContext(IInstanceIdentifier identification)
        {
            ServiceProvider = identification.ServiceProvider;
            Options = (EntityContextOptions)identification;
        }

        /// <summary>
        /// 获取应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// 获取 <see cref="EntityContext"/> 的参数。
        /// </summary>
        public EntityContextOptions Options { get; internal set; }
    }
}
