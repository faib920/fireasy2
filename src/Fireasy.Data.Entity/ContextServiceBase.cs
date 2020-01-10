// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Data.Provider;
using System;
using System.Data;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// <see cref="IContextService"/> 的抽象类。
    /// </summary>
    public abstract class ContextServiceBase :
        IContextService,
        IEntityPersistentInstanceContainer,
        IEntityPersistentEnvironment
    {
        private SafetyDictionary<Type, IRepositoryProvider> providers = new SafetyDictionary<Type, IRepositoryProvider>();

        /// <summary>
        /// 初始化 <see cref="ContextServiceBase"/> 类的新实例。
        /// </summary>
        /// <param name="context"></param>
        public ContextServiceBase(EntityContextInitializeContext context)
        {
            InitializeContext = context;
            InstanceName = ContextInstanceManager.TryAdd(context);
        }

        public string InstanceName { get; set; }

        public EntityPersistentEnvironment Environment { get; set; }

        public EntityContextInitializeContext InitializeContext { get; protected set; }

        public IProvider Provider { get; protected set; }

        public abstract void BeginTransaction(IsolationLevel level);

        public abstract void CommitTransaction();

        public abstract void RollbackTransaction();

        public abstract void Dispose();

        public virtual IRepositoryProvider CreateRepositoryProvider(Type entityType)
        {
            return providers.GetOrAdd(entityType, key => CreateFactory(key));
        }

        public virtual IRepositoryProvider<TEntity> CreateRepositoryProvider<TEntity>() where TEntity : class, IEntity
        {
            return (IRepositoryProvider<TEntity>)CreateRepositoryProvider(typeof(TEntity));
        }

        protected abstract Func<Type, IRepositoryProvider> CreateFactory { get; }
    }

    static class ContextServiceExtensions
    {
        public static IRepository CreateRepository(this IContextService service, Type entityType)
        {
            return service.CreateRepositoryProvider(entityType).CreateRepository(service.InitializeContext.Options);
        }
    }
}
