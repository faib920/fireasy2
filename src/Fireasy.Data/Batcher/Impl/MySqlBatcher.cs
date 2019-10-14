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
using System.Data.Common;
using System.Linq;
using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using Fireasy.Data.Syntax;
using Fireasy.Data.Provider;
using System.Threading.Tasks;
using System.Threading;

namespace Fireasy.Data.Batcher
{
    /// <summary>
    /// 为 MySql.Data 组件提供的用于批量操作的方法。无法继承此类。
    /// </summary>
    public sealed class MySqlBatcher : BatcherBase, IBatcherProvider
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
            throw new NotImplementedException();
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

            if (dataTable.IsNullOrEmpty())
            {
                return;
            }

            var mapping = GetNameTypeMapping(dataTable);
            await BatchInsertAsync(database, dataTable.Rows, dataTable.TableName, mapping, (map, command, r, item) => MapDataRow(database.Provider, map, (DataRow)item, r, command.Parameters), batchSize, completePercentage, cancellationToken);
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
            if (!BatcherChecker.CheckList(list, tableName))
            {
                return;
            }

            if (list.IsNullOrEmpty())
            {
                return;
            }

            await BatchInsertAsync(database, ToCollection(list), tableName, null, (map, command, r, item) => MapListItem<T>(database.Provider, map, (T)item, r, command.Parameters), batchSize, completePercentage, cancellationToken);
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// 批量插入集合中的数据。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="collection">要插入的数据的集合。</param>
        /// <param name="tableName">表的名称。</param>
        /// <param name="mapping">名称和类型的映射字典。</param>
        /// <param name="valueFunc">取值函数。</param>
        /// <param name="batchSize">每批次写入的数据量。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        private async Task BatchInsertAsync(IDatabase database, ICollection collection, string tableName, IList<PropertyFieldMapping> mapping, Func<IList<PropertyFieldMapping>, DbCommand, int, object, string> valueFunc, int batchSize, Action<int> completePercentage, CancellationToken cancellationToken = default)
        {
            //MySql使用如 insert into table(f1, f2) values ('a1', 'b1'),('a2', 'b2'),('a3', 'b3') 方式批量插入
            try
            {
                await database.Connection.TryOpenAsync();
                using (var command = database.Provider.CreateCommand(database.Connection, database.Transaction, null))
                {
                    var syntax = database.Provider.GetService<ISyntaxProvider>();
                    var valueSeg = new List<string>(batchSize);
                    var count = collection.Count;

                    BatchSplitData(collection, batchSize,
                        (index, batch, item) => 
                            {
                                if (mapping == null)
                                {
                                    mapping = GetNameTypeMapping(item);
                                }

                                valueSeg.Add(string.Format("({0})", valueFunc(mapping, command, batch, item)));
                            },
                        (index, batch, surplus, lastBatch) => 
                            {
                                var sql = string.Format("INSERT INTO {0}({1}) VALUES {2}",
                                    DbUtility.FormatByQuote(syntax, tableName),
                                    string.Join(",", mapping.Select(s => DbUtility.FormatByQuote(syntax, s.FieldName))), string.Join(",", valueSeg));

                                command.CommandText = sql;
                                command.ExecuteNonQueryAsync(cancellationToken);
                                valueSeg.Clear();
                                command.Parameters.Clear();
                                completePercentage?.Invoke((int)(((index + 1.0) / count) * 100));
                            });
                }
            }
            catch (Exception exp)
            {
                throw new BatcherException(collection, exp);
            }
        }
    }
}
