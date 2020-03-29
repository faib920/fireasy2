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
        private readonly MongoClient client;

        public MongoDBContextService(ContextServiceContext context)
            : base(context)
        {
            var connectionString = context.Options.ConnectionString;
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
        public MongoDatabase Database { get; private set; }

        /// <summary>
        /// 获取事务会话。
        /// </summary>
        public IClientSession Session { get; private set; }

        /// <summary>
        /// 开启事务。
        /// </summary>
        /// <param name="level"></param>
        public override void BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            Session = client.StartSession();
            Session.StartTransaction();
        }

        /// <summary>
        /// 提交事务。
        /// </summary>
        public override void CommitTransaction()
        {
            Session?.CommitTransaction();
        }

        /// <summary>
        /// 回滚事务。
        /// </summary>
        public override void RollbackTransaction()
        {
            Session?.AbortTransaction();
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        /// <param name="disposing"></param>
        protected override bool Dispose(bool disposing)
        {
            Session?.Dispose();

            return base.Dispose(disposing);
        }
    }
}
