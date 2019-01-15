// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Data;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 用于在实体持久化环境中创建 <see cref="IDatabase"/> 实例。
    /// </summary>
    public static class EntityDatabaseFactory
    {
        /// <summary>
        /// 检查当前的 <see cref="EntityTransactionScope"/> 实例，如果存在，则使用该对象所引用的 <see cref="IDatabase"/> 实例；
        /// 否则由 <see cref="DatabaseFactory"/> 工厂进行创建。
        /// </summary>
        /// <param name="instanceName">配置实例名称。</param>
        /// <returns>一个 <see cref="Database"/> 实例对象。</returns>
        public static IDatabase CreateDatabase(string instanceName = null)
        {
            if (EntityTransactionScope.Current == null)
            {
                return DatabaseScope.Current != null && DatabaseScope.Current.InstanceName == instanceName ? DatabaseScope.Current.Database :
                    DatabaseFactory.CreateDatabase(instanceName);
            }

            //首次请求不启动数据库事务
            var database = EntityTransactionScope.Current.GetDatabase(instanceName);
            if (database == null)
            {
                database = new EntityDatabase(DatabaseFactory.CreateDatabase(instanceName));
                EntityTransactionScope.Current.SetDatabase(instanceName, database);
                StartTransaction(database, EntityTransactionScope.Current.Option);
            }

            return database;
        }

        /// <summary>
        /// 启动数据库事务。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="option"></param>
        public static void StartTransaction(IDatabase database, EntityTransactionScopeOption option)
        {
            var isolationLevel = IsolationLevel.ReadUncommitted;
            if (option != null)
            {
                //设置超时时间
                if (option.Timeout != TimeSpan.Zero)
                {
                    database.Timeout = (int)option.Timeout.TotalSeconds;
                }

                isolationLevel = option.IsolationLevel;
            }

            database.BeginTransaction(isolationLevel);
        }
    }
}
