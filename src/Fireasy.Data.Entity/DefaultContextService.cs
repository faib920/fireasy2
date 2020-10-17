// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Threading;
using Fireasy.Data.Entity.Query;
using System;
using System.Data;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 核心的组件，用于管理上下文中的各种组件。
    /// </summary>
    public sealed class DefaultContextService :
        ContextServiceBase,
        IEntityTransactional,
        IQueryPolicyAware,
        IDatabaseAware
    {
        private readonly Func<IDatabase> _databaseCreateor;
        private IDatabase _database;

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
            _databaseCreateor = () => CreateDatabase(context);

            QueryPolicy = new DefaultQueryPolicy(Provider);
        }

        /// <summary>
        /// 获取数据库实例。
        /// </summary>
        public IDatabase Database
        {
            get
            {
                return SingletonLocker.Lock(ref _database, _databaseCreateor);
            }
        }

        public override IServiceProvider ServiceProvider
        {
            get { return base.ServiceProvider; }
            set
            {
                base.ServiceProvider = value;

                _database?.TrySetServiceProvider(value);
            }
        }

        protected override Func<Type, IRepositoryProvider> CreateFactory =>
            type => typeof(DefaultRepositoryProvider<>).MakeGenericType(type).New<IRepositoryProvider>(this);

        /// <summary>
        /// 释放资源。
        /// </summary>
        /// <param name="disposing"></param>
        protected override bool Dispose(bool disposing)
        {
            if (EntityTransactionScope.IsInTransaction())
            {
                return false;
            }

            RollbackTransaction();

            if (Database != null)
            {
                Database.TryDispose(disposing);
                _database = null;
            }

            return base.Dispose(disposing);
        }

        private IDatabase CreateDatabase(ContextServiceContext context)
        {
            var factory = context.ServiceProvider.TryGetService<IDatabaseFactory>();
            if (factory != null)
            {
                return factory.Create(context.Options);
            }

            IDatabase database = null;
            if (context.Options.Provider != null)
            {
                if (context.Options.DistributedConnectionStrings != null)
                {
                    database = new Database(context.Options.DistributedConnectionStrings, context.Options.Provider);
                }
                else if (context.Options.ConnectionString != null)
                {
                    database = new Database(context.Options.ConnectionString, context.Options.Provider);
                }

                database = database.TrySetServiceProvider(context.ServiceProvider);
            }
            else
            {
                throw new InvalidOperationException(SR.GetString(SRKind.NotSupportDatabaseFactory));
            }

            if (database != null && EntityTransactionScope.IsInTransaction())
            {
                database.BeginTransaction();
                EntityTransactionScope.Current.Addransaction((string)context.Options.ConnectionString, this);
            }

            return database;
        }

        /// <summary>
        /// 开始数据库事务。
        /// </summary>
        /// <param name="level"></param>
        public void BeginTransaction(IsolationLevel level)
        {
            Database.BeginTransaction();
        }

        /// <summary>
        /// 提交数据库事务。
        /// </summary>
        public void CommitTransaction()
        {
            if (EntityTransactionScope.IsInTransaction())
            {
                return;
            }

            Database.CommitTransaction();
        }

        /// <summary>
        /// 回滚数据库事务。
        /// </summary>
        public void RollbackTransaction()
        {
            if (EntityTransactionScope.IsInTransaction())
            {
                return;
            }

            Database.RollbackTransaction();
        }
    }
}
