// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Provider;
using System;
using System.Data;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 核心的组件，用于管理上下文中的各种组件。
    /// </summary>
    public sealed class DefaultContextService :
        ContextServiceBase,
        IQueryPolicyAware,
        IDatabaseAware
    {
        /// <summary>
        /// 获取 <see cref="IQueryPolicy"/> 实例。
        /// </summary>
        public IQueryPolicy QueryPolicy { get; private set; }

        /// <summary>
        /// 初始化 <see cref="DefaultContextService"/> 类的新实例。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="databaseFactory">一个用于创建 <see cref="IDatabase"/> 的工厂函数。</param>
        public DefaultContextService(EntityContextInitializeContext context, Func<IProvider, ConnectionString, IDatabase> databaseFactory)
            : base(context)
        {
            var options = context.Options;

            Func<IDatabase> factory = null;
            if (DatabaseScope.Current != null)
            {
                factory = () => DatabaseScope.Current.Database;
            }
            else if (databaseFactory != null)
            {
                factory = () => databaseFactory(context.Provider, context.ConnectionString);
            }
            else if (context.Provider != null && context.ConnectionString != null)
            {
                factory = () => new Database(context.ConnectionString, context.Provider);
            }
            else if (options != null)
            {
                factory = () => DatabaseFactory.CreateDatabase(options.ConfigName);
            }

            if (factory != null)
            {
                Database = EntityDatabaseFactory.CreateDatabase(InstanceName, factory);
                Provider = Database.Provider;
            }

            QueryPolicy = new DefaultQueryPolicy(Provider);
        }

        /// <summary>
        /// 获取 <see cref="IDatabase"/> 实例。
        /// </summary>
        public IDatabase Database { get; private set; }

        protected override Func<Type, IRepositoryProvider> CreateFactory => 
            type => typeof(DefaultRepositoryProvider<>).MakeGenericType(type).New<IRepositoryProvider>(this);

        /// <summary>
        /// 开始事务。
        /// </summary>
        /// <param name="level"></param>
        public override void BeginTransaction(IsolationLevel level)
        {
            Database?.BeginTransaction(level);
        }

        /// <summary>
        /// 提交事务。
        /// </summary>
        public override void CommitTransaction()
        {
            Database?.CommitTransaction();
        }

        /// <summary>
        /// 回滚事务。
        /// </summary>
        public override void RollbackTransaction()
        {
            Database?.RollbackTransaction();
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public override void Dispose()
        {
            Database?.Dispose();
        }
    }
}
