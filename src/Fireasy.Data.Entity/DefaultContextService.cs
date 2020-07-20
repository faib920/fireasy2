// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Query;
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
        public IQueryPolicy QueryPolicy { get; }

        /// <summary>
        /// 初始化 <see cref="DefaultContextService"/> 类的新实例。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="databaseFactory">一个用于创建 <see cref="IDatabase"/> 的工厂函数。</param>
        public DefaultContextService(ContextServiceContext context)
            : base(context)
        {
            Database = TryGetDatabase(context);
            QueryPolicy = new DefaultQueryPolicy(Provider);
        }

        /// <summary>
        /// 获取数据库实例。
        /// </summary>
        public IDatabase Database { get; private set; }

        public override IServiceProvider ServiceProvider
        {
            get { return base.ServiceProvider; }
            set
            {
                base.ServiceProvider = value;
                Database?.TrySetServiceProvider(value);
            }
        }

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
        /// <param name="disposing"></param>
        protected override bool Dispose(bool disposing)
        {
            if (Database != null)
            {
                Database.TryDispose(disposing);
                Database = null;
            }

            return base.Dispose(disposing);
        }

        private IDatabase TryGetDatabase(ContextServiceContext context)
        {
            var accessor = context.ServiceProvider.TryGetService<SharedDatabaseAccessor>();
            IDatabase database;

            if (accessor != null && (database = accessor[context.Options.ConnectionString]) != null)
            {
                return database;
            }

            Func<IDatabase> dbCreator;
            if (context.Options.Provider != null && context.Options.ConnectionString != null)
            {
                if (accessor == null)
                {
                    dbCreator = () => new ScopedDatabase(context.Options.ConnectionString, context.Options.Provider);
                }
                else
                {
                    dbCreator = () => new Database(context.Options.ConnectionString, context.Options.Provider);
                }
            }
            else
            {
                throw new InvalidOperationException(SR.GetString(SRKind.NotSupportDatabaseFactory));
            }

            database = EntityDatabaseFactory.CreateDatabase(InstanceName, dbCreator).TrySetServiceProvider(context.ServiceProvider);

            if (accessor != null)
            {
                accessor[context.Options.ConnectionString] = database;
            }

            return database;
        }
    }
}
