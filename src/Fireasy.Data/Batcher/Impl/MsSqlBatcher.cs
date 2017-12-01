// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Fireasy.Common;
using Fireasy.Data.Extensions;
using Fireasy.Data.Syntax;
using System.Reflection;

namespace Fireasy.Data.Batcher
{
    /// <summary>
    /// 为 System.Data.SqlClient 提供的用于批量操作的方法。无法继承此类。
    /// </summary>
    public sealed class MsSqlBatcher : IBatcherProvider
    {
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
            if (!BatcherChecker.CheckList(list, tableName))
            {
                return;
            }

            Insert(database, list.ToDataTable(tableName), batchSize);
        }

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

            try
            {
                database.Connection.TryOpen();

                //给表名加上前后导符
                var tableName = DbUtility.FormatByQuote(database.Provider.GetService<ISyntaxProvider>(), dataTable.TableName);
                using (var bulk = new SqlBulkCopy((SqlConnection)database.Connection, SqlBulkCopyOptions.KeepIdentity, null)
                {
                    DestinationTableName = tableName,
                    BatchSize = batchSize
                })
                {
#if !NETSTANDARD2_0
                    bulk.WriteToServer(dataTable);
#else
                    //todo 还未处理
#endif
                }
            }
            catch (Exception exp)
            {
                throw new BatcherException(dataTable.Rows, exp);
            }
        }
    }
}
