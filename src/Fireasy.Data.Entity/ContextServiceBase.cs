// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data;
using Fireasy.Data.Provider;

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
        private EntityRepositoryHolder holder = new EntityRepositoryHolder();

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

        public IDatabase Database { get; protected set; }

        public abstract void BeginTransaction(IsolationLevel level);

        public abstract void CommitTransaction();

        public abstract void RollbackTransaction();

        public abstract void Dispose();

        public virtual IRepository GetDbSet(Type entityType)
        {
            return holder.GetDbSet(this, entityType);
        }
    }
}
