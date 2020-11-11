// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Caching;
using Fireasy.Common.Threading;
using Fireasy.Data.Extensions;
using Fireasy.Data.Schema;
using Fireasy.Data.Syntax;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Fireasy.Data
{
    /// <summary>
    /// 分布式同步器抽象类。
    /// </summary>
    public abstract class DistributedSynchronizerBase : IDistributedSynchronizer
    {
        private bool? _isSyncTableExists = null;

        /// <summary>
        /// 获取同步器的表名。
        /// </summary>
        protected virtual string ClusterSyncTableName => "ClusterSync";

        /// <summary>
        /// 捕捉命令的执行。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="queryCommand"></param>
        public void CatchExecuting(IDistributedDatabase database, IQueryCommand queryCommand)
        {
            if (queryCommand is SpecialCommand || !CheckNeedProcess(database))
            {
                return;
            }

            var tableName = queryCommand.GetMainTableName(database.Provider.GetService<ISyntaxProvider>());
            if (!string.IsNullOrEmpty(tableName))
            {
                var cacheManager = CacheManagerFactory.CreateManager();
                var distributedLocker = DistributedLockerFactory.CreateLocker();

                var hashSet = cacheManager.GetHashSet<string, bool>(ClusterSyncTableName);
                hashSet.Remove(tableName);

                if (distributedLocker != null)
                {
                    distributedLocker.Lock($"{ClusterSyncTableName}:{tableName}", TimeSpan.FromSeconds(10), () =>
                    {
                        database.WithTransaction(db => TryAddSyncMarked(db, tableName));
                    });
                }
                else
                {
                    database.WithTransaction(db => TryAddSyncMarked(db, tableName));
                }
            }
        }

        /// <summary>
        /// 调整分布式模式。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="queryCommand"></param>
        public DistributedMode AdjustMode(IDistributedDatabase database, IQueryCommand queryCommand)
        {
            if (queryCommand is SpecialCommand || !CheckNeedProcess(database))
            {
                return DistributedMode.Slave;
            }

            var tableName = queryCommand.GetMainTableName(database.Provider.GetService<ISyntaxProvider>());
            if (!string.IsNullOrEmpty(tableName))
            {
                return CompareMasterAndSlave(database, tableName);
            }

            return DistributedMode.Slave;
        }

        private bool CheckNeedProcess(IDistributedDatabase database)
        {
            return database.DistributedConnectionStrings != null && database.DistributedConnectionStrings.Count > 0;
        }

        /// <summary>
        /// 比较主从两库的数据是否同步。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private DistributedMode CompareMasterAndSlave(IDistributedDatabase database, string tableName)
        {
            var cacheManager = CacheManagerFactory.CreateManager();

            var hashSet = cacheManager.GetHashSet<string, bool>(ClusterSyncTableName);
            if (hashSet.Contains(tableName))
            {
                return DistributedMode.Slave;
            }

            if (!CheckClusterSyncTableExists(database))
            {
                return DistributedMode.Slave;
            }

            var sql = $"SELECT RowVersion FROM {ClusterSyncTableName} WHERE TableName='{tableName}'";

            DbConnection conn1 = null;
            DbConnection conn2 = null;

            try
            {
                Tracer.Debug($"Query row-version of {tableName}.");
                conn1 = database.GetConnection(DistributedMode.Master);
                conn1.TryOpen();
                var command1 = conn1.CreateCommand();
                command1.CommandText = sql;
                var ver1 = command1.ExecuteScalar();

                conn2 = database.GetConnection(DistributedMode.Slave);
                conn2.TryOpen();
                var command2 = conn2.CreateCommand();
                command2.CommandText = sql;
                var ver2 = command2.ExecuteScalar();

                if (!Equals(ver1, ver2))
                {
                    return DistributedMode.Master;
                }

                hashSet.Add(tableName, true, new RelativeTime(TimeSpan.FromMinutes(5)));
            }
            finally
            {
                conn1?.TryClose();
                conn2?.TryClose();
            }

            return DistributedMode.Slave;
        }

        /// <summary>
        /// 获取创建触发器的 <see cref="SpecialCommand"/> 列表。
        /// </summary>
        /// <param name="clusterSyncTableName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        protected virtual IEnumerable<IQueryCommand> GetTriggerCommands(string clusterSyncTableName, string tableName)
        {
            yield break;
        }

        /// <summary>
        /// 尝试添加表的同步记录。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="tableName"></param>
        protected virtual void TryAddSyncMarked(IDatabase database, string tableName)
        {
            if (!CheckClusterSyncTableExists(database))
            {
                return;
            }

            SpecialCommand sql = $"SELECT COUNT(1) FROM {ClusterSyncTableName} WHERE TableName='{tableName}'";

            using var scope = new ForceUseMasterScope();

            if (database.ExecuteScalar<int>(sql) == 0)
            {
                var parameters = new ParameterCollection();
                parameters.Add("Name", tableName);
                parameters.Add("RowVersion", DateTime.Now);
                database.ExecuteNonQuery((SpecialCommand)$"INSERT INTO {ClusterSyncTableName} VALUES(@Name, @RowVersion)", parameters);

                Tracer.Debug($"Initialize triggers of {tableName}.");

                foreach (var sqlTrigger in GetTriggerCommands(ClusterSyncTableName, tableName))
                {
                    database.ExecuteNonQuery(sqlTrigger);
                }
            }
        }

        /// <summary>
        /// 检查 ClusterSync 表是否存在。
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        protected virtual bool CheckClusterSyncTableExists(IDatabase database)
        {
            if (_isSyncTableExists == null)
            {
                var schema = database.Provider.GetService<ISchemaProvider>();
                _isSyncTableExists = schema.GetSchemas<Table>(database, s => s.Name == ClusterSyncTableName).Any();
            }

            return (bool)_isSyncTableExists;
        }
    }
}
