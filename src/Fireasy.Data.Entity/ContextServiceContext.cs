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
    public struct ContextServiceContext
    {
        /// <summary>
        /// 初始化 <see cref="ContextServiceContext"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="identifier"></param>
        public ContextServiceContext(IServiceProvider serviceProvider, IInstanceIdentifier identifier)
        {
            ServiceProvider = serviceProvider;
            ContextType = identifier.ContextType;
            Options = (EntityContextOptions)identifier;
        }

        /// <summary>
        /// 初始化 <see cref="ContextServiceContext"/> 类的新实例。
        /// </summary>
        /// <param name="identifier"></param>
        public ContextServiceContext(IInstanceIdentifier identifier)
        {
            ServiceProvider = identifier.ServiceProvider;
            ContextType = identifier.ContextType;
            Options = (EntityContextOptions)identifier;
        }

        /// <summary>
        /// 获取 <see cref="EntityContext"/> 的类型。
        /// </summary>
        public Type ContextType { get; }

        /// <summary>
        /// 获取应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// 获取 <see cref="EntityContext"/> 的参数。
        /// </summary>
        public EntityContextOptions Options { get; internal set; }
    }
}
