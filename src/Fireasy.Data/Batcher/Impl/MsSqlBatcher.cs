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
            InsertAsync(database, dataTable, batchSize, completePercentage);
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
            InsertAsync(database, list, tableName, batchSize, completePercentage);
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
            InsertAsync(database, reader, tableName, batchSize, completePercentage);
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

            try
            {
                database.Connection.TryOpen();

                //给表名加上前后导符
                var tableName = DbUtility.FormatByQuote(database.Provider.GetService<ISyntaxProvider>(), dataTable.TableName);
                using (var bulk = new SqlBulkCopy((SqlConnection)database.Connection, SqlBulkCopyOptions.KeepIdentity, (SqlTransaction)database.Transaction)
                {
                    DestinationTableName = tableName,
                    BatchSize = batchSize
                })
                using (var reader = new DataTableBatchReader(bulk, dataTable))
                {
                    await bulk.WriteToServerAsync(reader, cancellationToken);
                }
            }
            catch (Exception exp)
            {
                throw new BatcherException(dataTable.Rows, exp);
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
            try
            {
                await database.Connection.TryOpenAsync(cancellationToken: cancellationToken);

                //给表名加上前后导符
                using (var bulk = new SqlBulkCopy((SqlConnection)database.Connection, SqlBulkCopyOptions.KeepIdentity, (SqlTransaction)database.Transaction)
                {
                    DestinationTableName = tableName,
                    BatchSize = batchSize
                })
                using (var reader = new EnumerableBatchReader<T>(bulk, list))
                {
                    await bulk.WriteToServerAsync(reader, cancellationToken);
                }
            }
            catch (Exception exp)
            {
                throw new BatcherException(list.ToList(), exp);
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
            try
            {
                await database.Connection.TryOpenAsync(cancellationToken: cancellationToken);

                //给表名加上前后导符
                using (var bulk = new SqlBulkCopy((SqlConnection)database.Connection, SqlBulkCopyOptions.KeepIdentity, (SqlTransaction)database.Transaction)
                {
                    DestinationTableName = tableName,
                    BatchSize = batchSize
                })
                await bulk.WriteToServerAsync((DbDataReader)reader, cancellationToken);
            }
            catch (Exception exp)
            {
                throw new BatcherException(null, exp);
            }
        }
    }
}
