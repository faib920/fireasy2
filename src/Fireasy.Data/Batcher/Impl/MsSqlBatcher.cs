// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Batcher
{
    /// <summary>
    /// 为 System.Data.SqlClient 提供的用于批量操作的方法。无法继承此类。
    /// </summary>
    public sealed class MsSqlBatcher : IBatcherProvider
    {
        IProvider IProviderService.Provider { get; set; }

        /// <summary>
        /// 将 <see cref="DataTable"/> 的数据批量插入到数据库中。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="dataTable">要批量插入的 <see cref="DataTable"/>。</param>
        /// <param name="batchSize">每批次写入的数据量。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public void Insert(IDatabase database, DataTable dataTable, int batchSize = 1000, Action<int> completePercentage = null)
        {
            if (!BatcherChecker.CheckDataTable(dataTable))
            {
                return;
            }

            ConnectionStateManager constateMgr = null;

            try
            {
                var connection = GetConnection(database);
                constateMgr = new ConnectionStateManager(connection);

                var syntax = database.Provider.GetService<ISyntaxProvider>();
                var tableName = syntax.DelimitTable(dataTable.TableName);
                using var bulk = GetBulkCopyProvider();
                bulk.Initialize(connection, database.Transaction, tableName, batchSize);
                using var reader = new DataTableBatchReader(dataTable, (i, n) => bulk.AddColumnMapping(i, n));
                bulk.WriteToServer(reader);
            }
            catch (Exception exp)
            {
                throw new BatcherException(dataTable.Rows, exp);
            }
            finally
            {
                constateMgr?.TryClose();
            }
        }

        /// <summary>
        /// 将一个 <see cref="IList"/> 批量插入到数据库中。 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="list">要写入的数据列表。</param>
        /// <param name="tableName">要写入的数据表的名称。</param>
        /// <param name="batchSize">每批次写入的数据量。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public void Insert<T>(IDatabase database, IEnumerable<T> list, string tableName, int batchSize = 1000, Action<int> completePercentage = null)
        {
            ConnectionStateManager constateMgr = null;

            try
            {
                var connection = GetConnection(database);
                constateMgr = new ConnectionStateManager(connection).TryOpen();

                using var bulk = GetBulkCopyProvider();
                bulk.Initialize(connection, database.Transaction, tableName, batchSize);
                using var reader = new EnumerableBatchReader<T>(list, (i, n) => bulk.AddColumnMapping(i, n));
                bulk.WriteToServer(reader);
            }
            catch (Exception exp)
            {
                throw new BatcherException(list.ToList(), exp);
            }
            finally
            {
                constateMgr?.TryClose();
            }
        }

        /// <summary>
        /// 将 <paramref name="reader"/> 中的数据流批量复制到数据库中。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="reader">源数据读取器。</param>
        /// <param name="tableName">要写入的数据表的名称。</param>
        /// <param name="batchSize">每批次写入的数据量。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public void Insert(IDatabase database, IDataReader reader, string tableName, int batchSize = 1000, Action<int> completePercentage = null)
        {
            ConnectionStateManager constateMgr = null;

            try
            {
                var connection = GetConnection(database);
                constateMgr = new ConnectionStateManager(connection).TryOpen();

                using var bulk = GetBulkCopyProvider();
                bulk.Initialize(connection, database.Transaction, tableName, batchSize);
                bulk.WriteToServer((DbDataReader)reader);
            }
            catch (Exception exp)
            {
                throw new BatcherException(null, exp);
            }
            finally
            {
                constateMgr?.TryClose();
            }
        }

        /// <summary>
        /// 将 <see cref="DataTable"/> 的数据批量插入到数据库中。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="dataTable">要批量插入的 <see cref="DataTable"/>。</param>
        /// <param name="batchSize">每批次写入的数据量。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public async Task InsertAsync(IDatabase database, DataTable dataTable, int batchSize = 1000, Action<int> completePercentage = null, CancellationToken cancellationToken = default)
        {
            if (!BatcherChecker.CheckDataTable(dataTable))
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            ConnectionStateManager constateMgr = null;

            try
            {
                var connection = GetConnection(database);
                constateMgr = await new ConnectionStateManager(connection).TryOpenAsync(cancellationToken);

                var syntax = database.Provider.GetService<ISyntaxProvider>();
                var tableName = syntax.DelimitTable(dataTable.TableName);
                using var bulk = GetBulkCopyProvider();
                bulk.Initialize(connection, database.Transaction, tableName, batchSize);
                using var reader = new DataTableBatchReader(dataTable, (i, n) => bulk.AddColumnMapping(i, n));
                await bulk.WriteToServerAsync(reader, cancellationToken);
            }
            catch (Exception exp)
            {
                throw new BatcherException(dataTable.Rows, exp);
            }
            finally
            {
                await constateMgr?.TryCloseAsync(cancellationToken);
            }
        }

        /// <summary>
        /// 将一个 <see cref="IList"/> 批量插入到数据库中。 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="list">要写入的数据列表。</param>
        /// <param name="tableName">要写入的数据表的名称。</param>
        /// <param name="batchSize">每批次写入的数据量。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public async Task InsertAsync<T>(IDatabase database, IEnumerable<T> list, string tableName, int batchSize = 1000, Action<int> completePercentage = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ConnectionStateManager constateMgr = null;

            try
            {
                var connection = GetConnection(database);
                constateMgr = await new ConnectionStateManager(connection).TryOpenAsync(cancellationToken);

                using var bulk = GetBulkCopyProvider();
                bulk.Initialize(connection, database.Transaction, tableName, batchSize);
                using var reader = new EnumerableBatchReader<T>(list, (i, n) => bulk.AddColumnMapping(i, n));
                await bulk.WriteToServerAsync(reader, cancellationToken);
            }
            catch (Exception exp)
            {
                throw new BatcherException(list.ToList(), exp);
            }
            finally
            {
                await constateMgr?.TryCloseAsync(cancellationToken);
            }
        }

        /// <summary>
        /// 将 <paramref name="reader"/> 中的数据流批量复制到数据库中。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="reader">源数据读取器。</param>
        /// <param name="tableName">要写入的数据表的名称。</param>
        /// <param name="batchSize">每批次写入的数据量。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public async Task InsertAsync(IDatabase database, IDataReader reader, string tableName, int batchSize = 1000, Action<int> completePercentage = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ConnectionStateManager constateMgr = null;

            try
            {
                var connection = GetConnection(database);
                constateMgr = await new ConnectionStateManager(connection).TryOpenAsync(cancellationToken);

                using var bulk = GetBulkCopyProvider();
                bulk.Initialize(connection, database.Transaction, tableName, batchSize);
                await bulk.WriteToServerAsync((DbDataReader)reader, cancellationToken);
            }
            catch (Exception exp)
            {
                throw new BatcherException(null, exp);
            }
            finally
            {
                await constateMgr?.TryCloseAsync(cancellationToken);
            }
        }

        private DbConnection GetConnection(IDatabase database)
        {
            return database is IDistributedDatabase distDb ? distDb.GetConnection(DistributedMode.Master) : database.Connection;
        }

        private IBulkCopyProvider GetBulkCopyProvider()
        {
            return (this as IProviderService).Provider.GetService<IBulkCopyProvider>() ?? new DefaultSqlBulkCopyProvider();
        }

        internal class DefaultSqlBulkCopyProvider : IBulkCopyProvider
        {
            private SqlBulkCopy _bulkCopy;

            IProvider IProviderService.Provider { get; set; }

            public void Dispose()
            {
                _bulkCopy.TryDispose();
            }

            public void Initialize(DbConnection connection, DbTransaction transaction, string tableName, int batchSize)
            {
                _bulkCopy = new SqlBulkCopy((SqlConnection)connection, SqlBulkCopyOptions.KeepIdentity, (SqlTransaction)transaction)
                {
                    DestinationTableName = tableName,
                    BatchSize = batchSize
                };
            }

            public void AddColumnMapping(int sourceColumnIndex, string destinationColumn)
            {
                _bulkCopy.ColumnMappings.Add(sourceColumnIndex, destinationColumn);
            }

            public void WriteToServer(DbDataReader reader)
            {
                _bulkCopy.WriteToServer(reader);
            }

            public async Task WriteToServerAsync(DbDataReader reader, CancellationToken cancellationToken)
            {
                await _bulkCopy.WriteToServerAsync(reader, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
