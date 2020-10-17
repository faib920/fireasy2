// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Data.Provider;
using System;
using System.Collections.Concurrent;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// <see cref="IContextService"/> 的抽象类。
    /// </summary>
    public abstract class ContextServiceBase :
        DisposableBase,
        IContextService,
        IServiceProviderAccessor,
        IEntityPersistentInstanceContainer,
        IEntityPersistentEnvironment
    {
        private readonly ConcurrentDictionary<Type, IRepositoryProvider> _providers = new ConcurrentDictionary<Type, IRepositoryProvider>();

        /// <summary>
        /// 初始化 <see cref="ContextServiceBase"/> 类的新实例。
        /// </summary>
        /// <param name="context"></param>
        protected ContextServiceBase(ContextServiceContext context)
        {
            Provider = context.Options.Provider;
            ContextType = context.ContextType;
            ServiceProvider = context.ServiceProvider;
            Options = context.Options;

            InstanceName = ContextInstanceManager.Default.TryAdd(context.Options);
        }

        /// <summary>
        /// 获取 <see cref="EntityContext"/> 的类型。
        /// </summary>
        public Type ContextType { get; }

        /// <summary>
        /// 获取或设置实例名称。
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// 获取或设置持久化环境实例。
        /// </summary>
        public EntityPersistentEnvironment Environment { get; set; }

        /// <summary>
        /// 获取 <see cref="EntityContextOptions"/> 实例。
        /// </summary>
        public EntityContextOptions Options { get; }

        /// <summary>
        /// 获取数据库提供者实例。
        /// </summary>
        public IProvider Provider { get; protected set; }

        /// <summary>
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        public virtual IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 创建实体类型所对应的 <see cref="IRepositoryProvider"/> 实例。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public virtual IRepositoryProvider CreateRepositoryProvider(Type entityType)
        {
            return _providers.GetOrAdd(entityType, key => CreateFactory(key));
        }

        /// <summary>
        /// 创建实体类型所对应的 <see cref="IRepositoryProvider{TEntity}"/> 实例。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public virtual IRepositoryProvider<TEntity> CreateRepositoryProvider<TEntity>() where TEntity : class, IEntity
        {
            return (IRepositoryProvider<TEntity>)CreateRepositoryProvider(typeof(TEntity));
        }

        /// <summary>
        /// <see cref="IRepositoryProvider"/> 的创建工厂。
        /// </summary>
        protected abstract Func<Type, IRepositoryProvider> CreateFactory { get; }

        protected override bool Dispose(bool disposing)
        {
            _providers.Clear();

            return base.Dispose(disposing);
        }
    }

    static class ContextServiceExtensions
    {
        internal static IRepository CreateRepository(this IContextService service, Type entityType)
        {
            return service.CreateRepositoryProvider(entityType).CreateRepository(service.Options);
        }
    }
}
