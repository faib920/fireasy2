// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity;
using MongoDB.Driver;
using System;
using System.Data;

namespace Fireasy.MongoDB
{
    public class MongoDBContextService : ContextServiceBase
    {
        private MongoClient client;

        public MongoDBContextService(EntityContextInitializeContext context)
            : base (context)
        {
            Provider = context.Provider;

            var connectionString = context.ConnectionString;
            var serverName = connectionString.Properties.TryGetValue("server");
            var database = connectionString.Properties.TryGetValue("database");
            if (string.IsNullOrEmpty(database))
            {
                database = "admin";
            }

            client = new MongoClient(serverName);
            Database = client.GetServer().GetDatabase(database);
        }

        protected override Func<Type, IRepositoryProvider> CreateFactory =>
            type => typeof(MongoDBRepositoryProvider<>).MakeGenericType(type).New<IRepositoryProvider>(this);

        /// <summary>
        /// 获取 <see cref="MongoDatabase"/> 对象。
        /// </summary>
        public new MongoDatabase Database { get; private set; }

        /// <summary>
        /// 获取事务会话。
        /// </summary>
        public IClientSession Session { get; private set; }

        public override void BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            Session = client.StartSession();
        }

        public override void CommitTransaction()
        {
            Session?.CommitTransaction();
        }

        public override void RollbackTransaction()
        {
            Session?.AbortTransaction();
        }

        public override void Dispose()
        {
            Session?.Dispose();
        }
    }
}
