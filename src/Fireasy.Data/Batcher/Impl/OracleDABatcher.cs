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
using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using Fireasy.Data.Converter;
using System.Threading.Tasks;
using System.Threading;

namespace Fireasy.Data.Batcher
{
    /// <summary>
    /// Oracle.Data.Access 组件提供的用于批量操作的方法。无法继承此类。
    /// </summary>
    public sealed class OracleDABatcher : BatcherBase, IBatcherProvider
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

            var bulkType = CheckBulkCopy(database.Provider);
            if (bulkType != null)
            {
                BulkCopy(database, bulkType, dataTable, batchSize);
            }
            else
            {
                var mapping = GetNameTypeMapping(dataTable);
                await BatchInsertAsync(database, dataTable.Rows, dataTable.TableName, mapping, batchSize, completePercentage, cancellationToken);
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
            if (!BatcherChecker.CheckList(list, tableName))
            {
                return;
            }

            var bulkType = CheckBulkCopy(database.Provider);
            if (bulkType != null)
            {
                BulkCopy(database, bulkType, list.ToDataTable(tableName), batchSize);
            }
            else
            {
                await BatchInsertAsync(database, ToCollection(list), tableName, null, batchSize, completePercentage, cancellationToken);
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// 批量插入集合中的数据。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="collection">要插入的数据的集合。</param>
        /// <param name="tableName">表的名称。</param>
        /// <param name="mapping">名称和类型的映射字典。</param>
        /// <param name="batchSize">每批次写入的数据量。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        private async Task BatchInsertAsync(IDatabase database, ICollection collection, string tableName, IList<PropertyFieldMapping> mapping, int batchSize, Action<int> completePercentage, CancellationToken cancellationToken = default)
        {
            //Oracle.DataAccess将每一列的数据构造成一个数组，然后使用参数进行插入
            try
            {
                await database.Connection.TryOpenAsync();
                using (var command = database.Provider.CreateCommand(database.Connection, database.Transaction, null))
                {
                    var syntax = database.Provider.GetService<ISyntaxProvider>();

                    var sql = string.Format("INSERT INTO {0}({1}) VALUES({2})",
                        DbUtility.FormatByQuote(syntax, tableName),
                        string.Join(",", mapping.Select(s => DbUtility.FormatByQuote(syntax, s.FieldName))), string.Join(",", mapping.Select(s => syntax.ParameterPrefix + s.FieldName)));

                    command.CommandText = sql;

                    var length = Math.Min(batchSize, collection.Count);
                    var count = collection.Count;
                    var data = InitArrayData(mapping.Count, length);
                    SetArrayBindCount(command, length);

                    BatchSplitData(collection, batchSize,
                        (index, batch, item) =>
                            {
                                if (mapping == null)
                                {
                                    mapping = GetNameTypeMapping(item);
                                }

                                FillArrayData(mapping, item, data, batch);
                            },
                        (index, batch, surplus, lastBatch) =>
                            {
                                AddOrReplayParameters(syntax, mapping, command.Parameters, data,
                                    () => database.Provider.DbProviderFactory.CreateParameter());

                                command.ExecuteNonQueryAsync(cancellationToken);
                                completePercentage?.Invoke((int)(((index + 1.0) / count) * 100));

                                if (!lastBatch)
                                {
                                    length = Math.Min(batchSize, surplus);
                                    data = InitArrayData(mapping.Count, length);
                                    SetArrayBindCount(command, length);
                                }
                            });
                }
            }
            catch (Exception exp)
            {
                throw new BatcherException(collection, exp);
            }
        }

        /// <summary>
        /// 检查是否可以使用OracleBulkCopy类执行批量插入。
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        private Type CheckBulkCopy(IProvider provider)
        {
            return provider.DbProviderFactory.GetType().Assembly
                .GetType("Oracle.DataAccess.Client.OracleBulkCopy", false);
        }

        /// <summary>
        /// 使用OracleBulkCopy类执行批量插入。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="bulkType">程序集中 OracleBulkCopy 类型。</param>
        /// <param name="table">要插入的数据表。</param>
        /// <param name="batchSize">每批次写入的数据量。</param>
        /// <returns></returns>
        private bool BulkCopy(IDatabase database, Type bulkType, DataTable table, int batchSize)
        {
            using (var connection = database.CreateConnection())
            using (var bulk = bulkType.New<IDisposable>(connection))
            {
                bulkType.GetProperty("DestinationTableName").AssertNotNull(s => s.SetValue(bulk, table.TableName, null));
                bulkType.GetProperty("BatchSize").AssertNotNull(s => s.SetValue(bulk, batchSize, null));

                try
                {
                    connection.OpenClose(() =>
                        {
                            bulkType.GetMethod("WriteToServer").AssertNotNull(s => s.Invoke(bulk, new object[] { table }));
                        });

                    return true;
                }
                catch
                {
                	return false;
                }
            }
        }

        /// <summary>
        /// 设置ArrayBindCount属性。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="batchSize"></param>
        private void SetArrayBindCount(DbCommand command, int batchSize)
        {
            command.GetType().GetProperty("ArrayBindCount").AssertNotNull(s => s.SetValue(command, batchSize, null));
        }

        /// <summary>
        /// 创建一个二维数组。
        /// </summary>
        /// <param name="columns">字段的个数。</param>
        /// <param name="length">元素的个数。</param>
        /// <returns></returns>
        private object[][] InitArrayData(int columns, int length)
        {
            var data = new object[columns][];
            for (var i = 0; i < columns; i++)
            {
                data[i] = new object[length];
            }

            return data;
        }

        /// <summary>
        /// 使用当前的记录填充数组。
        /// </summary>
        /// <param name="mappings">名称和类型的映射字典。</param>
        /// <param name="item">当前的数据项。</param>
        /// <param name="data">数组的数组。</param>
        /// <param name="batch">当前批次中的索引。</param>
        private void FillArrayData(IEnumerable<PropertyFieldMapping> mappings, object item, object[][] data, int batch)
        {
            var i = 0;
            foreach (var map in mappings)
            {
                var value = map.ValueFunc(item);
                if (value != null)
                {
                    var converter = ConvertManager.GetConverter(map.PropertyType);
                    if (converter != null)
                    {
                        value = converter.ConvertTo(value, map.FieldType);
                    }
                }

                data[i++][batch] = value;
            }
        }

        /// <summary>
        /// 添加或替换集合中的参数。
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="mapping">名称和类型的映射字典。</param>
        /// <param name="parameters"><see cref="DbCommand"/> 中的参数集合。</param>
        /// <param name="data">数组的数组。</param>
        /// <param name="parFunc">创建 <see cref="DbParameter"/> 对象的函数。</param>
        private void AddOrReplayParameters(ISyntaxProvider syntax, IEnumerable<PropertyFieldMapping> mapping, DbParameterCollection parameters, object[][] data, Func<DbParameter> parFunc)
        {
            var i = 0;
            foreach (var kvp in mapping)
            {
                if (parameters.Contains(kvp.FieldName))
                {
                    parameters[kvp.FieldName].Value = data[i];
                }
                else
                {
                    var parameter = parFunc();
                    parameter.ParameterName = kvp.FieldName;
                    parameter.Direction = ParameterDirection.Input;
                    parameter.DbType = kvp.FieldType;
                    parameter.Value = data[i];
                    parameters.Add(parameter);
                }

                i++;
            }
        }
    }
}
