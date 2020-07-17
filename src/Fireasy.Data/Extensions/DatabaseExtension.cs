// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.Extensions;
using System;
using System.Data;
using System.Data.Common;

namespace Fireasy.Data.Extensions
{
    /// <summary>
    /// <see cref="IDatabase"/> 的扩展方法。
    /// </summary>
    public static class DatabaseExtension
    {
        /// <summary>
        /// 开启一个事务，用来执行一组方法。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/>。</param>
        /// <param name="actExec">要执行的一组方法。</param>
        /// <param name="funCatch">捕获异常，如果返回 true 则仍抛出异常。</param>
        /// <param name="level">事务的级别。</param>
        public static void WithTransaction(this IDatabase database, Action<IDatabase> actExec, Func<IDatabase, Exception, bool> funCatch = null, IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            if (actExec == null)
            {
                return;
            }

            database.BeginTransaction(level);
            try
            {
                actExec(database);
                database.CommitTransaction();
            }
            catch (Exception ex)
            {
                database.RollbackTransaction();
                if (funCatch != null)
                {
                    if (funCatch(database, ex))
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 创建一个新的 <see cref="DbConnection"/> 对象。
        /// </summary>
        /// <param name="database"><see cref="IDatabase"/> 对象。</param>
        /// <param name="mode">分布式模式。</param>
        /// <returns><paramref name="database"/> 创建的 <see cref="DbConnection"/> 对象。</returns>
        public static DbConnection CreateConnection(this IDatabase database, DistributedMode mode = DistributedMode.Master)
        {
            Guard.ArgumentNull(database, nameof(database));

            ConnectionString connStr = null;

            if (mode == DistributedMode.Slave && database is IDistributedDatabase distDb)
            {
                var serviceProvider = database.TryGetServiceProvider();
                var manager = serviceProvider.TryGetService(() => DefaultDistributedConnectionManager.Instance);

                connStr = manager.GetConnection(distDb);
            }

            return database.Provider.CreateConnection((connStr ?? database.ConnectionString).ToString());
        }
    }
}
