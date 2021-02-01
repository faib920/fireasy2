// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common.Threading;
using Fireasy.Data.Entity.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 核心的组件，用于管理上下文中的各种组件。
    /// </summary>
    public sealed class DefaultContextService :
        ContextServiceBase,
        IEntityTransactional,
        IEntityBatchExecutable,
        IQueryPolicyAware,
        IDatabaseAware,
        IObjectPoolNotifyChain
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
                if (_databaseCreateor == null)
                {
                    return null;
                }

                return SingletonLocker.Lock(ref _database, this, _databaseCreateor);
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
                var hasInterceptor = context.Options.ServiceProvider.GetService(typeof(DbCommandInterceptor)) != null;

                if (context.Options.DistributedConnectionStrings != null)
                {
                    database = hasInterceptor ?
                        new InterceptedDatabase(context.Options.DistributedConnectionStrings, context.Options.Provider) :
                        new Database(context.Options.DistributedConnectionStrings, context.Options.Provider);
                }
                else if (context.Options.ConnectionString != null)
                {
                    database = hasInterceptor ?
                        new InterceptedDatabase(context.Options.ConnectionString, context.Options.Provider) :
                        new Database(context.Options.ConnectionString, context.Options.Provider);
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

        /// <summary>
        /// 处理批处理命令。
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="parameters"></param>
        public void ExecuteBatch(IEnumerable<string> commands, ParameterCollection parameters)
        {
            Database.ExecuteBatch(commands.Select(s => (SqlCommand)s), parameters);
        }

        /// <summary>
        /// 异步的，处理批处理命令。
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ExecuteBatchAsync(IEnumerable<string> commands, ParameterCollection parameters, CancellationToken cancellationToken = default)
        {
            return Database.ExecuteBatchAsync(commands.Select(s => (SqlCommand)s), parameters, cancellationToken);
        }

        void IObjectPoolNotifyChain.OnReturn()
        {
            if (Database is IObjectPoolNotifyChain chain)
            {
                chain.OnReturn();
            }
        }
    }
}
