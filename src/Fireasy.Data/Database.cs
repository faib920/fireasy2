// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using Fireasy.Data.Identity;
using Fireasy.Data.Provider;
using Fireasy.Data.RecordWrapper;
using Fireasy.Data.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data
{
    /// <summary>
    /// 提供数据库基本操作的方法。
    /// </summary>
    public class Database : IDatabase, IDistributedDatabase
    {
        private readonly TransactionStack tranStack;
        private bool isDisposed;
        private readonly DatabaseScope dbScope;
        private DbConnection connMaster;
        private DbConnection connSlave;

        /// <summary>
        /// 初始化 <see cref="Database"/> 类的新实例。
        /// </summary>
        protected Database()
        {
            tranStack = new TransactionStack();
            dbScope = new DatabaseScope(this);
            Track = DefaultCommandTracker.Instance;
        }

        /// <summary>
        /// 初始化 <see cref="Database"/> 类的新实例。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串。</param>
        /// <param name="provider">数据库提供者。</param>
        public Database(ConnectionString connectionString, IProvider provider)
            : this()
        {
            Guard.ArgumentNull(provider, nameof(provider));
            Provider = provider;
            ConnectionString = connectionString;
        }

        /// <summary>
        /// 初始化 <see cref="Database"/> 类的新实例。
        /// </summary>
        /// <param name="connectionStrings">数据库连接字符串组。</param>
        /// <param name="provider">数据库提供者。</param>
        public Database(List<DistributedConnectionString> connectionStrings, IProvider provider)
            : this()
        {
            Guard.ArgumentNull(provider, nameof(provider));
            Provider = provider;
            DistributedConnectionStrings = connectionStrings.ToReadOnly();
            ConnectionString = connectionStrings.Find(s => s.Mode == DistributedMode.Master);
        }

        /// <summary>
        /// 析构函数。
        /// </summary>
        ~Database()
        {
            Dispose(false);
        }

        /// <summary>
        /// 获取或设置数据库连接字符串。
        /// </summary>
        public ConnectionString ConnectionString { get; set; }

        /// <summary>
        /// 获取分布式数据库连接字符串组。
        /// </summary>
        public ReadOnlyCollection<DistributedConnectionString> DistributedConnectionStrings { get; private set; }

        /// <summary>
        /// 获取数据库提供者。
        /// </summary>
        public IProvider Provider { get; private set; }

        /// <summary>
        /// 获取或设置命令执行的跟踪器。
        /// </summary>
        public ICommandTracker Track { get; set; }

        /// <summary>
        /// 获取或设置日志函数。
        /// </summary>
        public Action<IDbCommand, TimeSpan> Log { get; set; }

        /// <summary>
        /// 获取或设置超时时间。
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// 获取当前数据库事务。
        /// </summary>
        public DbTransaction Transaction { get; private set; }

        /// <summary>
        /// 获取当前数据库链接。
        /// </summary>
        public DbConnection Connection
        {
            get
            {
                var connection = connMaster ?? connSlave;
                if (connection == null)
                {
                    connection = GetConnection();
                }

                return connection;
            }
        }

        /// <summary>
        /// 使用指定锁定行为启动一个数据库事务。
        /// </summary>
        /// <param name="level">事务的锁定行为。</param>
        /// <returns>如果当前实例首次启动事务，则为 true，否则为 false。</returns>
        public virtual bool BeginTransaction(IsolationLevel level = IsolationLevel.ReadUncommitted)
        {
            tranStack.Push();
            if (Transaction != null)
            {
                return false;
            }

            if (TransactionScopeConnections.GetConnection(this) != null)
            {
                return false;
            }

            Connection.TryOpen();
            Transaction = Connection.BeginTransaction(level);

            return true;
        }

        /// <summary>
        /// 如果与方法 BeginTransaction 匹配，则提交数据库事务。
        /// </summary>
        /// <returns>成功提交事务则为 true，否则为 false。</returns>
        public virtual bool CommitTransaction()
        {
            if (Transaction == null ||
                !tranStack.Pop())
            {
                return false;
            }

            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
            return true;
        }

        /// <summary>
        /// 如果与方法 BeginTransaction 匹配，则回滚数据库事务。
        /// </summary>
        /// <returns>成功回滚事务则为 true，否则为 false。</returns>
        public virtual bool RollbackTransaction()
        {
            if (Transaction == null ||
                !tranStack.Pop())
            {
                return false;
            }

            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
            return true;
        }

        /// <summary>
        /// 执行查询文本并将结果填充到指定的 <see cref="DataTable"/> 对象中。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="tableName"><see cref="DataTable"/> 的名称。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>一个 <see cref="DataTable"/> 对象。</returns>
        public virtual DataTable ExecuteDataTable(IQueryCommand queryCommand, string tableName = null, IDataSegment segment = null, ParameterCollection parameters = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));
            var ds = new DataSet();
            FillDataSet(ds, queryCommand, tableName, segment, parameters);
            return ds.Tables.Count == 0 ? null : ds.Tables[0];
        }

        /// <summary>
        /// 执行查询文本并将结果以一个 <see cref="IEnumerable{T}"/> 的序列返回。
        /// </summary>
        /// <typeparam name="T">查询对象类型。</typeparam>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="rowMapper">数据行映射器。</param>
        /// <returns>一个 <typeparamref name="T"/> 类型的对象的枚举器。</returns>
        public virtual IEnumerable<T> ExecuteEnumerable<T>(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, IDataRowMapper<T> rowMapper = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));

            rowMapper = rowMapper ?? RowMapperFactory.CreateRowMapper<T>();
            rowMapper.RecordWrapper = Provider.GetService<IRecordWrapper>();
            using (var reader = ExecuteReader(queryCommand, segment, parameters))
            {
                while (reader.Read())
                {
                    yield return rowMapper.Map(this, reader);
                }
            }
        }

        /// <summary>
        /// 根据自定义的SQL语句查询返回一组动态对象。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>一个动态对象的枚举器。</returns>
        public virtual IEnumerable<dynamic> ExecuteEnumerable(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));

            using (var reader = ExecuteReader(queryCommand, segment, parameters))
            {
                var wrapper = Provider.GetService<IRecordWrapper>();
                TypeDescriptorUtility.AddDefaultDynamicProvider();

                while (reader.Read())
                {
                    var expando = new ExpandoObject();
                    var dictionary = (IDictionary<string, object>)expando;

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var name = wrapper.GetFieldName(reader, i);
                        if (name.Equals("ROW_NUM"))
                        {
                            continue;
                        }

                        dictionary.Add(wrapper.GetFieldName(reader, i), RecordWrapHelper.GetValue(wrapper, reader, i));
                    }

                    yield return expando;
                }
            }
        }

#if NETSTANDARD && !NETSTANDARD2_0
        /// <summary>
        /// 异步的，执行查询文本并将结果以一个 <see cref="IEnumerable{T}"/> 的序列返回。
        /// </summary>
        /// <typeparam name="T">查询对象类型。</typeparam>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="rowMapper">数据行映射器。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>一个 <typeparamref name="T"/> 类型的对象的枚举器。</returns>
        public async virtual IAsyncEnumerable<T> ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, IDataRowMapper<T> rowMapper = null, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));

            rowMapper = rowMapper ?? RowMapperFactory.CreateRowMapper<T>();
            rowMapper.RecordWrapper = Provider.GetService<IRecordWrapper>();
            using (var reader = (DbDataReader)(await ExecuteReaderAsync(queryCommand, segment, parameters, cancellationToken)))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    yield return rowMapper.Map(this, reader);
                }
            }
        }

        /// <summary>
        /// 异步的，执行查询文本并将结果并返回动态序列。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>一个动态对象的枚举器。</returns>
        public virtual async IAsyncEnumerable<dynamic> ExecuteEnumerableAsync(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));

            using (var reader = (DbDataReader)(await ExecuteReaderAsync(queryCommand, segment, parameters, cancellationToken)))
            {
                var wrapper = Provider.GetService<IRecordWrapper>();
                TypeDescriptorUtility.AddDefaultDynamicProvider();

                while (await reader.ReadAsync(cancellationToken))
                {
                    var expando = new ExpandoObject();
                    var dictionary = (IDictionary<string, object>)expando;

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var name = wrapper.GetFieldName(reader, i);
                        if (name.Equals("ROW_NUM"))
                        {
                            continue;
                        }

                        dictionary.Add(wrapper.GetFieldName(reader, i), RecordWrapHelper.GetValue(wrapper, reader, i));
                    }

                    yield return expando;
                }
            }
        }
#else
        /// <summary>
        /// 异步的，执行查询文本并将结果以一个 <see cref="IEnumerable{T}"/> 的序列返回。
        /// </summary>
        /// <typeparam name="T">查询对象类型。</typeparam>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="rowMapper">数据行映射器。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>一个 <typeparamref name="T"/> 类型的对象的枚举器。</returns>
        public async virtual Task<IEnumerable<T>> ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, IDataRowMapper<T> rowMapper = null, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));

            var result = new List<T>();
            rowMapper = rowMapper ?? RowMapperFactory.CreateRowMapper<T>();
            rowMapper.RecordWrapper = Provider.GetService<IRecordWrapper>();
            using (var reader = (DbDataReader)(await ExecuteReaderAsync(queryCommand, segment, parameters, cancellationToken)))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(rowMapper.Map(this, reader));
                }
            }

            return result;
        }

        /// <summary>
        /// 异步的，执行查询文本并将结果并返回动态序列。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>一个动态对象的枚举器。</returns>
        public virtual async Task<IEnumerable<dynamic>> ExecuteEnumerableAsync(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));

            var result = new List<dynamic>();
            using (var reader = await ExecuteReaderAsync(queryCommand, segment, parameters))
            {
                var wrapper = Provider.GetService<IRecordWrapper>();
                TypeDescriptorUtility.AddDefaultDynamicProvider();

                while (await ((DbDataReader)reader).ReadAsync(cancellationToken))
                {
                    var expando = new ExpandoObject();
                    var dictionary = (IDictionary<string, object>)expando;

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var name = wrapper.GetFieldName(reader, i);
                        if (name.Equals("ROW_NUM"))
                        {
                            continue;
                        }

                        dictionary.Add(wrapper.GetFieldName(reader, i), RecordWrapHelper.GetValue(wrapper, reader, i));
                    }

                    result.Add(expando);
                }
            }

            return result;
        }
#endif

        /// <summary>
        /// 执行查询文本，返回受影响的记录数。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>所影响的记录数。</returns>
        public virtual int ExecuteNonQuery(IQueryCommand queryCommand, ParameterCollection parameters = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));
            return UsingConnection(connection =>
                {
                    using (var command = CreateDbCommand(connection, queryCommand, parameters))
                    {
                        try
                        {
                            return AfterCommandExecuted(command, parameters, () => command.ExecuteNonQuery());
                        }
                        catch (DbException exp)
                        {
                            HandleFailedLogAsync(command, exp);
                            throw new CommandException(command, exp);
                        }
                    }
                }, mode: DistributedMode.Master);
        }

        /// <summary>
        /// 异步的，执行查询文本，返回受影响的记录数。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>所影响的记录数。</returns>
        public async virtual Task<int> ExecuteNonQueryAsync(IQueryCommand queryCommand, ParameterCollection parameters = null, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));
            return await UsingConnectionAsync(async connection =>
                {
                    using (var command = CreateDbCommand(connection, queryCommand, parameters))
                    {
                        try
                        {
                            return await AfterCommandExecutedAsync(command, parameters, () => command.ExecuteNonQueryAsync(cancellationToken));
                        }
                        catch (DbException exp)
                        {
                            await HandleFailedLogAsync(command, exp);
                            throw new CommandException(command, exp);
                        }
                    }
                }, mode: DistributedMode.Master, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行查询文本并返回一个 <see cref="IDataReader"/>。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>一个 <see cref="IDataReader"/> 对象。</returns>
        public virtual IDataReader ExecuteReader(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));
            return UsingConnection(connection =>
                {
                    var command = CreateDbCommand(connection, queryCommand, parameters);
                    try
                    {
                        var cmdBehavior = CommandBehavior.Default;
                        var context = new CommandContext(this, command, segment, parameters);
                        HandleSegmentCommand(context);
                        return AfterCommandExecuted(command, parameters, () => command.ExecuteReader(cmdBehavior));
                    }
                    catch (DbException exp)
                    {
                        HandleFailedLogAsync(command, exp);
                        throw new CommandException(command, exp);
                    }
                }, mode: DistributedMode.Slave);
        }

        /// <summary>
        /// 异步的，执行查询文本并返回一个 <see cref="IDataReader"/>。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>一个 <see cref="IDataReader"/> 对象。</returns>
        public async virtual Task<IDataReader> ExecuteReaderAsync(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));
            return await UsingConnectionAsync(async connection =>
                {
                    var command = CreateDbCommand(connection, queryCommand, parameters);
                    try
                    {
                        var cmdBehavior = CommandBehavior.Default;
                        var context = new CommandContext(this, command, segment, parameters);
                        HandleSegmentCommand(context);

                        return await AfterCommandExecutedAsync(command, parameters, () => command.ExecuteReaderAsync(cmdBehavior, cancellationToken));
                    }
                    catch (DbException exp)
                    {
                        await HandleFailedLogAsync(command, exp);
                        throw new CommandException(command, exp);
                    }
                }, mode: DistributedMode.Slave, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行查询文本，并返回第一行的第一列。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>第一行的第一列数据。</returns>
        public virtual object ExecuteScalar(IQueryCommand queryCommand, ParameterCollection parameters = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));
            return UsingConnection(connection =>
                {
                    using (var command = CreateDbCommand(connection, queryCommand, parameters))
                    {
                        try
                        {
                            return AfterCommandExecuted(command, parameters, () => command.ExecuteScalar());
                        }
                        catch (DbException exp)
                        {
                            HandleFailedLogAsync(command, exp);
                            throw new CommandException(command, exp);
                        }
                    }
                }, mode: DistributedMode.Slave);
        }

        /// <summary>
        /// 执行查询文本，并返回第一行的第一列。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>第一行的第一列数据。</returns>
        public virtual T ExecuteScalar<T>(IQueryCommand queryCommand, ParameterCollection parameters = null)
        {
            var result = ExecuteScalar(queryCommand, parameters);
            if (result != DBNull.Value)
            {
                return result.To<T>();
            }

            return default(T);
        }

        /// <summary>
        /// 异步的，执行查询文本，并返回第一行的第一列。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>第一行的第一列数据。</returns>
        public async virtual Task<object> ExecuteScalarAsync(IQueryCommand queryCommand, ParameterCollection parameters = null, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));
            return await UsingConnectionAsync(async connection =>
                {
                    using (var command = CreateDbCommand(connection, queryCommand, parameters))
                    {
                        try
                        {
                            return await AfterCommandExecutedAsync(command, parameters, () => command.ExecuteScalarAsync(cancellationToken));
                        }
                        catch (DbException exp)
                        {
                            await HandleFailedLogAsync(command, exp);
                            throw new CommandException(command, exp);
                        }
                    }
                }, mode: DistributedMode.Slave, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 异步的，执行查询文本，并返回第一行的第一列。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>第一行的第一列数据。</returns>
        public async virtual Task<T> ExecuteScalarAsync<T>(IQueryCommand queryCommand, ParameterCollection parameters = null, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteScalarAsync(queryCommand, parameters, cancellationToken);

            return result == DBNull.Value ? default(T) : result.To<T>();
        }

        /// <summary>
        /// 执行查询文本并将结果填充到指定的 <see cref="DataSet"/> 对象中。
        /// </summary>
        /// <param name="dataSet">要填充的 <see cref="DataSet"/>。</param>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="tableName">表的名称，多个表名称使用逗号分隔。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        public virtual void FillDataSet(DataSet dataSet, IQueryCommand queryCommand, string tableName = null, IDataSegment segment = null, ParameterCollection parameters = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));
            var adapter = Provider.DbProviderFactory.CreateDataAdapter();
            if (adapter == null)
            {
                throw new NotSupportedException(nameof(DataAdapter));
            }

            UsingConnection(connection =>
                {
                    using (var command = CreateDbCommand(connection, queryCommand, parameters))
                    {
                        adapter.SelectCommand = command;

                        //如果要使用Update更新DataSet，则必须指定MissingSchemaAction.AddWithKey，
                        //但在Oracle使用分页时，却不能设置该属性，否则抛出“应为标识符或带引号的标识符”
                        //因此，如果要实现Update，只有手动添加DataSet的PrimaryKeys
                        //adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                        dataSet.EnforceConstraints = false;
                        HandleAdapterTableMapping(adapter, tableName);

                        try
                        {
                            var context = new CommandContext(this, command, segment, parameters);

                            //无法分页时才采用 adapter.Fill(dataSet, startRecord, maxRecords, "Table")
                            var handler = !HandleSegmentCommand(context) && segment != null ?
                                new Func<int>(() => adapter.Fill(dataSet, segment.Start.Value, segment.Length, "Table")) :
                                new Func<int>(() => adapter.Fill(dataSet));

                            return AfterCommandExecuted(command, parameters, handler);
                        }
                        catch (DbException exp)
                        {
                            HandleFailedLogAsync(command, exp);

                            throw new CommandException(command, exp);
                        }
                    }
                }, false, DistributedMode.Slave);
        }

        /// <summary>
        /// 尝试连接数据库。
        /// </summary>
        /// <returns>如果连接成功，则为 null，否则为异常对象。</returns>
        public virtual Exception TryConnect()
        {
            using (var connection = this.CreateConnection())
            {
                try
                {
                    connection.TryOpen();
                    return null;
                }
                catch (DbException exp)
                {
                    return exp;
                }
                finally
                {
                    connection.TryClose();
                }
            }
        }

        /// <summary>
        /// 异步的，尝试连接数据库。
        /// </summary>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果连接成功，则为 null，否则为异常对象。</returns>
        public virtual async Task<Exception> TryConnectAsync(CancellationToken cancellationToken = default)
        {
            using (var connection = this.CreateConnection())
            {
                try
                {
                    await connection.TryOpenAsync(cancellationToken: cancellationToken);
                    return null;
                }
                catch (DbException exp)
                {
                    return exp;
                }
                finally
                {
                    connection.TryClose();
                }
            }
        }

        /// <summary>
        /// 将 <see cref="DataTable"/> 的更改保存到数据库中，这类更改包括新增、修改和删除的数据。
        /// </summary>
        /// <param name="dataTable">要更新的数据表对象。</param>
        public void Update(DataTable dataTable)
        {
            Guard.ArgumentNull(dataTable, nameof(dataTable));

            UsingConnection(connection =>
                {
                    var builder = new CommandBuilder(Provider, dataTable, connection, Transaction);
                    var adapter = Provider.DbProviderFactory.CreateDataAdapter();
                    if (adapter == null)
                    {
                        throw new NotSupportedException(nameof(DataAdapter));
                    }

                    builder.FillAdapter(adapter);
                    return adapter.Update(dataTable);
                }, false, DistributedMode.Master);
        }

        /// <summary>
        /// 将 <see cref="DataTable"/> 的更改保存到数据库中，这类更改包括新增、修改和删除的数据。
        /// </summary>
        /// <param name="dataTable">要更新的数据表对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task UpdateAsync(DataTable dataTable, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => Update(dataTable), cancellationToken);
        }

        /// <summary>
        /// 将 <see cref="DataTable"/> 的更改保存到数据库中。
        /// </summary>
        /// <param name="dataTable">要更新的数据表对象。</param>
        /// <param name="insertCommand"></param>
        /// <param name="updateCommand"></param>
        /// <param name="deleteCommand"></param>
        /// <returns></returns>
        public int Update(DataTable dataTable, SqlCommand insertCommand, SqlCommand updateCommand, SqlCommand deleteCommand)
        {
            Guard.ArgumentNull(dataTable, nameof(dataTable));

            return UsingConnection(connection =>
                {
                    var parameters = GetTableParameters(dataTable);
                    var adapter = Provider.DbProviderFactory.CreateDataAdapter();
                    if (adapter == null)
                    {
                        return UpdateManually(dataTable, parameters, insertCommand, updateCommand, deleteCommand);
                    }

                    if (insertCommand != null)
                    {
                        adapter.InsertCommand = CreateDbCommand(connection, insertCommand, parameters);
                        adapter.InsertCommand.UpdatedRowSource = UpdateRowSource.Both;
                    }

                    ProcessGenerateDataTable(dataTable);

                    return adapter.Update(dataTable);

                }, false, DistributedMode.Master);
        }

        /// <summary>
        /// 将 <see cref="DataTable"/> 的更改保存到数据库中。
        /// </summary>
        /// <param name="dataTable">要更新的数据表对象。</param>
        /// <param name="insertCommand"></param>
        /// <param name="updateCommand"></param>
        /// <param name="deleteCommand"></param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(DataTable dataTable, SqlCommand insertCommand, SqlCommand updateCommand, SqlCommand deleteCommand, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Update(dataTable, insertCommand, updateCommand, deleteCommand), cancellationToken);
        }

        /// <summary>
        /// 释放对象所占用的所有资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (Transaction != null)
            {
                Transaction.Dispose();
                Transaction = null;
            }

            if (connMaster != null)
            {
                connMaster.Dispose();
                connMaster = null;
            }

            if (connSlave != null)
            {
                connSlave.Dispose();
                connSlave = null;
            }

            dbScope.Dispose();
            isDisposed = true;
        }

        /// <summary>
        /// 通知应用程序，<see cref="DbConnection"/> 的状态已经改变。
        /// </summary>
        /// <param name="originalState">原来的状态。</param>
        /// <param name="currentState">当前的状态。</param>
        protected virtual void OnConnectionStateChanged(ConnectionState originalState, ConnectionState currentState)
        {
        }

        /// <summary>
        /// 创建一个 DbCommand 对象。
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">参数集合。</param>
        /// <returns>一个由 <see cref="IQueryCommand"/> 和参数集合组成的 <see cref="DbCommand"/> 对象。</returns>
        private DbCommand CreateDbCommand(DbConnection connection, IQueryCommand queryCommand, IEnumerable<Parameter> parameters)
        {
            var command = connection.CreateCommand();
            Guard.NullReference(command);
            command.CommandType = queryCommand.GetCommandType();
            command.CommandText = HandleCommandParameterPrefix(queryCommand.ToString());
            command.Transaction = Transaction;
            if (Timeout != 0)
            {
                command.CommandTimeout = Timeout;
            }

            if (parameters != null)
            {
                command.PrepareParameters(Provider, parameters);
            }

            Provider.PrepareCommand(command);

            return command;
        }

        private T UsingConnection<T>(Func<DbConnection, T> callback, bool isAutoOpen = true, DistributedMode mode = DistributedMode.Master)
        {
            var result = default(T);
            if (callback != null)
            {
                var connection = GetConnection(mode);
                connection.TryOpen(isAutoOpen);
                result = callback(connection);
            }

            return result;
        }

        private async Task<T> UsingConnectionAsync<T>(Func<DbConnection, Task<T>> callback, bool isAutoOpen = true, DistributedMode mode = DistributedMode.Master, CancellationToken cancellationToken = default)
        {
            if (callback != null)
            {
                var connection = GetConnection(mode);
                await connection.TryOpenAsync(isAutoOpen, cancellationToken);
                return await callback(connection);
            }

            return default(T);
        }

        private DbConnection GetConnection(DistributedMode mode = DistributedMode.Master)
        {
            if (Transaction != null)
            {
                return Transaction.Connection;
            }
            else
            {
                var connection = TransactionScopeConnections.GetConnection(this);
                if (connection != null)
                {
                    return connection;
                }

                var isNew = false;

                if (mode == DistributedMode.Slave)
                {
                    if (connSlave == null)
                    {
                        connection = connSlave = Provider.PrepareConnection(this.CreateConnection(mode));
                        isNew = true;
                    }
                    else
                    {
                        connection = connSlave;
                    }
                }
                else if (mode == DistributedMode.Master)
                {
                    if (connMaster == null)
                    {
                        connection = connMaster = Provider.PrepareConnection(this.CreateConnection(mode));
                        isNew = true;
                    }
                    else
                    {
                        connection = connMaster;
                    }
                }

                if (isNew)
                {
                    connection.StateChange += (o, e) => OnConnectionStateChanged(e.OriginalState, e.CurrentState);
                }

                return connection;
            }
        }

        /// <summary>
        /// 处理表名映射。
        /// </summary>
        /// <param name="adapter">适配器。</param>
        /// <param name="tableName">表的名称。</param>
        private void HandleAdapterTableMapping(IDataAdapter adapter, string tableName)
        {
            const string defaultTableName = "Table";

            //处理表名
            if (string.IsNullOrEmpty(tableName))
            {
                adapter.TableMappings.Add(defaultTableName, defaultTableName);
            }
            else if (tableName.IndexOf(',') != -1)
            {
                //如果使用|连接多个表名
                //命名为Table、Table1、Table2...
                const string sysTableNameRoot = defaultTableName;
                var tableNames = tableName.Split(',');
                for (var i = 0; i < tableNames.Length; i++)
                {
                    var sysTableName = i == 0 ? sysTableNameRoot : sysTableNameRoot + i;
                    adapter.TableMappings.Add(sysTableName, tableNames[i]);
                }
            }
            else
            {
                adapter.TableMappings.Add(defaultTableName, tableName);
            }
        }

        /// <summary>
        /// 格式化执行的SQL脚本，将 @ 替换为对应数据库的参数符号。
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        private string HandleCommandParameterPrefix(string commandText)
        {
            var syntax = Provider.GetService<ISyntaxProvider>();
            if (string.IsNullOrEmpty(syntax.ParameterPrefix))
            {
                return commandText;
            }

            if (Regex.IsMatch(commandText, "(\\" + syntax.ParameterPrefix + ")"))
            {
                return commandText;
            }

            if (syntax != null && !syntax.ParameterPrefix.Equals("@") && Regex.IsMatch(commandText, "(@)"))
            {
                return Regex.Replace(commandText, "(@)", syntax.ParameterPrefix.ToString());
            }

            return commandText;
        }

        /// <summary>
        /// 对执行的SQL脚本使用分页参数。
        /// </summary>
        /// <param name="context"></param>
        private bool HandleSegmentCommand(CommandContext context)
        {
            //使用数据分段
            if (context.Segment != null &&
                context.Command.CommandType == CommandType.Text)
            {
                try
                {
                    var syntax = Provider.GetService<ISyntaxProvider>();
                    HandlePageEvaluator(context);

                    var newCommandText = syntax.Segment(context);
                    context.Command.CommandText = newCommandText;
                    return true;
                }
                catch (SegmentNotSupportedException)
                {
                    throw;
                }
            }

            return false;
        }

        /// <summary>
        /// 处理分页评估器。
        /// </summary>
        /// <param name="context"></param>
        private void HandlePageEvaluator(CommandContext context)
        {
            if (context.Segment is IDataPageEvaluatable evaluatable && evaluatable.Evaluator != null)
            {
                evaluatable.Evaluator.Evaluate(context);
            }
        }

        /// <summary>
        /// 处理错误日志。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="exp"></param>
        private async Task HandleFailedLogAsync(DbCommand command, Exception exp, CancellationToken cancellationToken = default)
        {
            if (ConnectionString.IsTracking && Track != null)
            {
                await Track.FailAsync(command, exp, cancellationToken);
            }
        }

        /// <summary>
        /// 处理日志。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="period"></param>
        private async Task HandleLogAsync(IDbCommand command, TimeSpan period, CancellationToken cancellationToken = default)
        {
            Log?.Invoke(command, period);

            if (ConnectionString.IsTracking && Track != null)
            {
                await Track.WriteAsync(command, period, cancellationToken);
            }
        }

        private ParameterCollection GetTableParameters(DataTable table)
        {
            var parameters = new ParameterCollection();
            foreach (DataColumn column in table.Columns)
            {
                var par = new Parameter(column.ColumnName) { SourceColumn = column.ColumnName };
                parameters.Add(par);
            }

            return parameters;
        }

        private int UpdateManually(DataTable dataTable, ParameterCollection parameters, SqlCommand insertCommand, SqlCommand updateCommand, SqlCommand deleteCommand)
        {
            if (updateCommand == null && deleteCommand == null && insertCommand != null)
            {
                return UpdateSimple(dataTable, parameters, insertCommand);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private int UpdateSimple(DataTable dataTable, ParameterCollection parameters, SqlCommand sqlCommand)
        {
            const string COLUMN_RESULT = "_Result";

            if (dataTable.Columns[COLUMN_RESULT] == null)
            {
                dataTable.Columns.Add(COLUMN_RESULT, typeof(int));
            }

            return UsingConnection(connection =>
                {
                    BeginTransaction();

                    using (var command = CreateDbCommand(connection, sqlCommand, parameters))
                    {
                        try
                        {
                            var result = 0;
                            foreach (DataRow row in dataTable.Rows)
                            {
                                UpdateParameters(command.Parameters, row);

                                var time = TimeWatcher.Watch(() => row[COLUMN_RESULT] = command.ExecuteScalar() ?? 0);
                                HandleLogAsync(command, time);

                                result++;
                            }

                            CommitTransaction();

                            return result;
                        }
                        catch (DbException exp)
                        {
                            HandleFailedLogAsync(command, exp);

                            RollbackTransaction();

                            throw new CommandException(command, exp);
                        }
                    }
                }, mode: DistributedMode.Master);
        }

        private void UpdateParameters(DbParameterCollection parameters, DataRow row)
        {
            foreach (DbParameter parameter in parameters)
            {
                if (row.Table.Columns[parameter.ParameterName] != null)
                {
                    parameter.Value = row[parameter.ParameterName];
                }
            }
        }

        /// <summary>
        /// 通知应用程序，一个 <see cref="DbCommand"/> 已经执行。
        /// </summary>
        /// <param name="command">所执行的 <see cref="DbCommand"/> 对象。</param>
        /// <param name="func">执行的方法。</param>
        private T AfterCommandExecuted<T>(DbCommand command, ParameterCollection parameters, Func<T> func)
        {
            var result = default(T);
            var period = TimeWatcher.Watch(() => result = func());

            HandleLogAsync(command, period);

            command.SyncParameters(parameters);
            command.ClearParameters();

            return result;
        }

        /// <summary>
        /// 异步的，通知应用程序，一个 <see cref="DbCommand"/> 已经执行。
        /// </summary>
        /// <param name="command">所执行的 <see cref="DbCommand"/> 对象。</param>
        /// <param name="parameters"></param>
        /// <param name="func">执行的方法。</param>
        private async Task<T> AfterCommandExecutedAsync<T>(DbCommand command, ParameterCollection parameters, Func<Task<T>> func)
        {
            var result = default(T);
            var period = await TimeWatcher.WatchAsync(async () => result = await func());

            await HandleLogAsync(command, period);

            command.SyncParameters(parameters);
            command.ClearParameters();

            return result;
        }

        /// <summary>
        /// 处理使用了 <see cref="IGeneratorProvider"/> 生成数据的数据表。
        /// </summary>
        /// <param name="dataTable"></param>
        private void ProcessGenerateDataTable(DataTable dataTable)
        {
            DataColumn genColumn = null;
            DataColumn newColumn = null;
            for (var i = dataTable.Columns.Count - 1; i >= 0; i--)
            {
                var column = dataTable.Columns[i];

                Type parType;
                if ((parType = DataExpressionColumn.GetParameterType(column.DataType)) != null)
                {
                    newColumn = dataTable.Columns.Add(column.ColumnName + "_NEW", parType);
                    genColumn = column;
                }
            }

            if (genColumn != null && newColumn != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    var id = (row[genColumn] as DataExpressionColumn).Setter.DynamicInvoke(this);
                    row[genColumn.ColumnName + "_NEW"] = id;
                }

                //移除原来的，新的改名
                dataTable.Columns.Remove(genColumn);
                newColumn.ColumnName = genColumn.ColumnName;
            }
        }
    }
}