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
using Fireasy.Data.Internal;
using Fireasy.Data.Provider;
using Fireasy.Data.RecordWrapper;
using Fireasy.Data.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
#if NETSTANDARD2_1
using System.Runtime.CompilerServices;
#endif
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data
{
    /// <summary>
    /// 提供数据库基本操作的方法。
    /// </summary>
    public class Database : DisposableBase, IDatabase, IDistributedDatabase, IServiceProviderAccessor
    {
        private DbConnection _connMaster;
        private DbConnection _connSlave;
        private DbTransaction _transaction;
        private DbTransactionScope _transactionScope;
        private readonly ReaderNestedlocked _readerLocker = new ReaderNestedlocked();

        /// <summary>
        /// 初始化 <see cref="Database"/> 类的新实例。
        /// </summary>
        /// <param name="provider">数据库提供者。</param>
        protected Database(IProvider provider)
        {
            Provider = provider;
        }

        /// <summary>
        /// 初始化 <see cref="Database"/> 类的新实例。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串。</param>
        /// <param name="provider">数据库提供者。</param>
        public Database(ConnectionString connectionString, IProvider provider)
            : this(provider)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// 初始化 <see cref="Database"/> 类的新实例。
        /// </summary>
        /// <param name="connectionStrings">数据库连接字符串组。</param>
        /// <param name="provider">数据库提供者。</param>
        public Database(List<DistributedConnectionString> connectionStrings, IProvider provider)
            : this(provider)
        {
            DistributedConnectionStrings = connectionStrings.ToReadOnly();
            ConnectionString = connectionStrings.Find(s => s.Mode == DistributedMode.Master);
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
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 获取数据库提供者。
        /// </summary>
        public IProvider Provider { get; private set; }

        /// <summary>
        /// 获取或设置超时时间。
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// 获取当前数据库事务。
        /// </summary>
        public DbTransaction Transaction
        {
            get
            {
                return _transaction ?? DbTransactionScope.Current?.GetCurrentTransaction(ConnectionString);
            }
        }

        /// <summary>
        /// 获取当前数据库链接。
        /// </summary>
        public DbConnection Connection
        {
            get
            {
                var connection = _connMaster ?? _connSlave;
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
        public virtual bool BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            if (Transaction != null)
            {
                return false;
            }

            if (TransactionScopeConnections.GetConnection(this) != null)
            {
                return false;
            }

            Tracer.Debug("Starting transcation.");

            var connection = GetConnection(DistributedMode.Master);
            connection.TryOpen();

            _transaction = connection.BeginTransaction(Provider.AmendIsolationLevel(level));
            _transactionScope = new DbTransactionScope(ConnectionString, _transaction);

            return true;
        }

        /// <summary>
        /// 如果与方法 BeginTransaction 匹配，则提交数据库事务。
        /// </summary>
        /// <returns>成功提交事务则为 true，否则为 false。</returns>
        public virtual bool CommitTransaction()
        {
            if (_transactionScope == null)
            {
                return false;
            }

            Tracer.Debug("Commiting transcation.");

            _transaction.Commit();
            _connMaster?.TryClose();
            _transactionScope.Dispose();
            _transaction = null;
            _transactionScope = null;

            return true;
        }

        /// <summary>
        /// 如果与方法 BeginTransaction 匹配，则回滚数据库事务。
        /// </summary>
        /// <returns>成功回滚事务则为 true，否则为 false。</returns>
        public virtual bool RollbackTransaction()
        {
            if (_transactionScope == null)
            {
                return false;
            }

            Tracer.Debug("Rollbacking transcation.");

            _transaction.Rollback();
            _connMaster?.TryClose();
            _transactionScope.Dispose();
            _transaction = null;
            _transactionScope = null;

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

            rowMapper ??= RowMapperFactory.CreateRowMapper<T>();
            rowMapper.RecordWrapper = Provider.GetService<IRecordWrapper>();
            using var reader = ExecuteReader(queryCommand, segment, parameters, CommandBehavior.Default);
            while (reader.Read())
            {
                yield return rowMapper.Map(this, reader);
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

            using var reader = ExecuteReader(queryCommand, segment, parameters, CommandBehavior.Default);
            var wrapper = Provider.GetService<IRecordWrapper>();
            TypeDescriptorUtility.AddDefaultDynamicProvider();

            while (reader.Read())
            {
                var expando = new ExpandoObject();
                var dictionary = (IDictionary<string, object>)expando;

                for (int i = 0, n = reader.FieldCount; i < n; i++)
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
        public async virtual IAsyncEnumerable<T> ExecuteAsyncEnumerable<T>(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, IDataRowMapper<T> rowMapper = null, [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Guard.ArgumentNull(queryCommand, nameof(queryCommand));

            rowMapper ??= RowMapperFactory.CreateRowMapper<T>();
            rowMapper.RecordWrapper = Provider.GetService<IRecordWrapper>();
            using var reader = (InternalDataReader)await ExecuteReaderAsync(queryCommand, segment, parameters, CommandBehavior.Default, cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                yield return rowMapper.Map(this, reader);
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
        public virtual async IAsyncEnumerable<dynamic> ExecuteAsyncEnumerable(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));

            using var reader = (InternalDataReader)await ExecuteReaderAsync(queryCommand, segment, parameters, CommandBehavior.Default, cancellationToken);
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
#endif
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
            cancellationToken.ThrowIfCancellationRequested();

            var result = new List<T>();
            rowMapper ??= RowMapperFactory.CreateRowMapper<T>();
            rowMapper.RecordWrapper = Provider.GetService<IRecordWrapper>();

            using var reader = (InternalDataReader)await ExecuteReaderAsync(queryCommand, segment, parameters, CommandBehavior.Default, cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(rowMapper.Map(this, reader));
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
            cancellationToken.ThrowIfCancellationRequested();

            var result = new List<dynamic>();
            using var reader = (InternalDataReader)await ExecuteReaderAsync(queryCommand, segment, parameters, CommandBehavior.Default, cancellationToken);
            var wrapper = Provider.GetService<IRecordWrapper>();
            TypeDescriptorUtility.AddDefaultDynamicProvider();

            while (await reader.ReadAsync(cancellationToken))
            {
                var expando = new ExpandoObject();
                var dictionary = (IDictionary<string, object>)expando;

                for (int i = 0, n = reader.FieldCount; i < n; i++)
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

            return result;
        }

        /// <summary>
        /// 执行查询文本，返回受影响的记录数。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>所影响的记录数。</returns>
        public virtual int ExecuteNonQuery(IQueryCommand queryCommand, ParameterCollection parameters = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));

            InitiaizeDistributedSynchronizer(queryCommand);

            var connection = GetConnection(DistributedMode.Master).TryOpen();

            using var command = CreateDbCommand(connection, queryCommand, parameters);
            try
            {
                return HandleCommandExecuted(command, parameters, CommandBehavior.Default, (command, behavior) => command.ExecuteNonQuery());
            }
            catch (DbException exp)
            {
                throw HandleException(command, exp);
            }
            finally
            {
                connection.TryClose(Transaction == null);
            }
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
            cancellationToken.ThrowIfCancellationRequested();

            InitiaizeDistributedSynchronizer(queryCommand);

            var connection = await GetConnection(DistributedMode.Master).TryOpenAsync();

            using var command = CreateDbCommand(connection, queryCommand, parameters);
            try
            {
                return await HandleCommandExecutedAsync(command, parameters, CommandBehavior.Default,
                    (command, behavior, cancelToken) => ((DbCommand)command).ExecuteNonQueryAsync(cancelToken), cancellationToken);
            }
            catch (DbException exp)
            {
                throw await HandleExceptionAsync(command, exp, cancellationToken);
            }
            finally
            {
                connection.TryClose(Transaction == null);
            }
        }

        /// <summary>
        /// 执行查询文本并返回一个 <see cref="IDataReader"/>。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="behavior"></param>
        /// <returns>一个 <see cref="IDataReader"/> 对象。</returns>
        public virtual IDataReader ExecuteReader(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, CommandBehavior? behavior = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));

            var mode = CheckForceUseMaster(() => AdjustMode(queryCommand));

            var connection = GetConnection(mode);

            var command = new InternalDbCommand(CreateDbCommand(connection, queryCommand, parameters), _readerLocker);
            try
            {
                var context = new CommandContext(this, queryCommand, command, segment, parameters);
                HandleSegmentCommand(context);
                return HandleCommandExecuted(command, parameters, behavior ?? CommandBehavior.Default, (command, behavior) => command.ExecuteReader(behavior));
            }
            catch (DbException exp)
            {
                throw HandleException(command, exp);
            }
        }

        /// <summary>
        /// 异步的，执行查询文本并返回一个 <see cref="IDataReader"/>。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="behavior"></param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>一个 <see cref="IDataReader"/> 对象。</returns>
        public async virtual Task<IDataReader> ExecuteReaderAsync(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, CommandBehavior? behavior = null, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));
            cancellationToken.ThrowIfCancellationRequested();

            var mode = CheckForceUseMaster(() => AdjustMode(queryCommand));

            var connection = GetConnection(mode);

            var command = new InternalDbCommand(CreateDbCommand(connection, queryCommand, parameters), _readerLocker);
            try
            {
                var context = new CommandContext(this, queryCommand, command, segment, parameters);
                await HandleSegmentCommandAsync(context, cancellationToken);

                return await HandleCommandExecutedAsync(command, parameters, behavior ?? CommandBehavior.Default,
                    (command, behavior, cancelToken) => command.ExecuteReaderAsync(behavior, cancelToken), cancellationToken);
            }
            catch (DbException exp)
            {
                throw await HandleExceptionAsync(command, exp, cancellationToken);
            }
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

            var mode = DistributedMode.Slave;
            if (queryCommand is IIdenticalSqlCommand)
            {
                mode = DistributedMode.Master;
                InitiaizeDistributedSynchronizer(queryCommand);
            }
            else
            {
                mode = CheckForceUseMaster(() => AdjustMode(queryCommand));
            }

            var connection = GetConnection(mode).TryOpen();

            using var command = CreateDbCommand(connection, queryCommand, parameters);
            try
            {
                return HandleCommandExecuted(command, parameters, CommandBehavior.Default, (command, behavior) => command.ExecuteScalar());
            }
            catch (DbException exp)
            {
                throw HandleException(command, exp);
            }
            finally
            {
                connection.TryClose(Transaction == null);
            }
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

            return default;
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
            cancellationToken.ThrowIfCancellationRequested();

            var mode = DistributedMode.Slave;
            if (queryCommand is IIdenticalSqlCommand)
            {
                mode = DistributedMode.Master;
                InitiaizeDistributedSynchronizer(queryCommand);
            }
            else
            {
                mode = CheckForceUseMaster(() => AdjustMode(queryCommand));
            }

            var connection = await GetConnection(mode).TryOpenAsync();

            using var command = CreateDbCommand(connection, queryCommand, parameters);
            try
            {
                return await HandleCommandExecutedAsync(command, parameters, CommandBehavior.Default,
                    (command, behavior, cancelToken) => ((DbCommand)command).ExecuteScalarAsync(cancelToken), cancellationToken);
            }
            catch (DbException exp)
            {
                throw await HandleExceptionAsync(command, exp, cancellationToken);
            }
            finally
            {
                connection.TryClose(Transaction == null);
            }
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
            cancellationToken.ThrowIfCancellationRequested();

            var result = await ExecuteScalarAsync(queryCommand, parameters, cancellationToken);

            return result == DBNull.Value ? default : result.To<T>();
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

            var mode = CheckForceUseMaster(() => AdjustMode(queryCommand));
            var connection = GetConnection(mode).TryOpen();

            using var command = CreateDbCommand(connection, queryCommand, parameters);
            adapter.SelectCommand = command;

            //如果要使用Update更新DataSet，则必须指定MissingSchemaAction.AddWithKey，
            //但在Oracle使用分页时，却不能设置该属性，否则抛出“应为标识符或带引号的标识符”
            //因此，如果要实现Update，只有手动添加DataSet的PrimaryKeys
            //adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            dataSet.EnforceConstraints = false;
            HandleAdapterTableMapping(adapter, tableName);

            try
            {
                var context = new CommandContext(this, queryCommand, command, segment, parameters);

                //无法分页时才采用 adapter.Fill(dataSet, startRecord, maxRecords, "Table")
                if (segment != null && !HandleSegmentCommand(context))
                {
                    adapter.Fill(dataSet, segment.Start.Value, segment.Length, "Table");
                }
                else
                {
                    adapter.Fill(dataSet);

                }
            }
            catch (DbException exp)
            {
                throw HandleException(command, exp);
            }
            finally
            {
                connection.TryClose();
            }
        }

        /// <summary>
        /// 尝试连接数据库。
        /// </summary>
        /// <returns>如果连接成功，则为 null，否则为异常对象。</returns>
        public virtual Exception TryConnect()
        {
            using var connection = this.CreateConnection();
            try
            {
                connection.TryOpen();
                return null;
            }
            catch (DbException exp)
            {
                return exp;
            }
        }

        /// <summary>
        /// 异步的，尝试连接数据库。
        /// </summary>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果连接成功，则为 null，否则为异常对象。</returns>
        public virtual async Task<Exception> TryConnectAsync(CancellationToken cancellationToken = default)
        {
            using var connection = this.CreateConnection();
            try
            {
                await connection.TryOpenAsync(cancellationToken: cancellationToken);
                return null;
            }
            catch (DbException exp)
            {
                return exp;
            }
        }

        /// <summary>
        /// 将 <see cref="DataTable"/> 的更改保存到数据库中，这类更改包括新增、修改和删除的数据。
        /// </summary>
        /// <param name="dataTable">要更新的数据表对象。</param>
        public void Update(DataTable dataTable)
        {
            Guard.ArgumentNull(dataTable, nameof(dataTable));
            var connection = GetConnection(DistributedMode.Master).TryOpen();

            var builder = new CommandBuilder(Provider, dataTable, connection, Transaction);
            var adapter = Provider.DbProviderFactory.CreateDataAdapter();
            if (adapter == null)
            {
                throw new NotSupportedException(nameof(DataAdapter));
            }

            try
            {
                builder.FillAdapter(adapter);
                adapter.Update(dataTable);
            }
            finally
            {
                connection.TryClose(Transaction == null);
            }
        }

        /// <summary>
        /// 将 <see cref="DataTable"/> 的更改保存到数据库中，这类更改包括新增、修改和删除的数据。
        /// </summary>
        /// <param name="dataTable">要更新的数据表对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task UpdateAsync(DataTable dataTable, CancellationToken cancellationToken = default)
        {
            Update(dataTable);
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
            var connection = GetConnection(DistributedMode.Master).TryOpen();

            HandleDynamicDataTable(dataTable);

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

            try
            {
                var tracker = ServiceProvider.TryGetService<ICommandTracker>(() => DefaultCommandTracker.Instance);

                var watch = Stopwatch.StartNew();
                var result = adapter.Update(dataTable);
                Tracer.Debug($"The DbDataAdapter was executed ({watch.Elapsed.Milliseconds}ms):\n{adapter.InsertCommand.Output()}");
                watch.Stop();

                if (ConnectionString.IsTracking && tracker != null)
                {
                    tracker?.Write(adapter.InsertCommand, watch.Elapsed);
                }

                return result;
            }
            catch (Exception exp)
            {
                throw HandleException(adapter.InsertCommand, exp);
            }
            finally
            {
                connection.TryClose(Transaction == null);
            }
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
            return Update(dataTable, insertCommand, updateCommand, deleteCommand);
        }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        protected override bool Dispose(bool disposing)
        {
            Tracer.Debug("The Database is Disposing.");

            RollbackTransaction();

            if (_connMaster != null)
            {
                _connMaster.TryClose();

                if (disposing)
                {
                    _connMaster.Dispose();
                    _connMaster = null;
                }
            }

            if (_connSlave != null)
            {
                _connSlave.TryClose();

                if (disposing)
                {
                    _connSlave.Dispose();
                    _connSlave = null;
                }
            }

            return base.Dispose(disposing);
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
        [SuppressMessage("Security", "CA2100")]
        private DbCommand CreateDbCommand(DbConnection connection, IQueryCommand queryCommand, ParameterCollection parameters)
        {
            var command = connection.CreateCommand();
            Guard.NullReference(command);
            command.CommandType = queryCommand.CommandType;
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

        DbConnection IDistributedDatabase.GetConnection(DistributedMode mode)
        {
            return GetConnection(mode);
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
                    if (_connSlave == null)
                    {
                        connection = _connSlave = Provider.PrepareConnection(this.CreateConnection(mode));
                        isNew = true;
                    }
                    else
                    {
                        connection = _connSlave;
                    }
                }
                else if (mode == DistributedMode.Master)
                {
                    if (_connMaster == null)
                    {
                        connection = _connMaster = Provider.PrepareConnection(this.CreateConnection(mode));
                        isNew = true;
                    }
                    else
                    {
                        connection = _connMaster;
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
                for (int i = 0, n = tableNames.Length; i < n; i++)
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
                return Regex.Replace(commandText, "(@)", syntax.ParameterPrefix);
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
                    return syntax.Segment(HandlePageEvaluator(context));
                }
                catch (SegmentNotSupportedException)
                {
                    throw;
                }
            }

            return false;
        }

        /// <summary>
        /// 对执行的SQL脚本使用分页参数。
        /// </summary>
        /// <param name="context"></param>
        private async Task<bool> HandleSegmentCommandAsync(CommandContext context, CancellationToken cancellationToken)
        {
            //使用数据分段
            if (context.Segment != null &&
                context.Command.CommandType == CommandType.Text)
            {
                try
                {
                    var syntax = Provider.GetService<ISyntaxProvider>();
                    return syntax.Segment(await HandlePageEvaluatorAsync(context, cancellationToken));
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
        private CommandContext HandlePageEvaluator(CommandContext context)
        {
            if (context.Segment is IDataPageEvaluatable evaluatable && evaluatable.Evaluator != null)
            {
                evaluatable.Evaluator.Evaluate(context);
            }

            return context;
        }

        /// <summary>
        /// 异步的，处理分页评估器。
        /// </summary>
        /// <param name="context"></param>
        private async Task<CommandContext> HandlePageEvaluatorAsync(CommandContext context, CancellationToken cancellationToken)
        {
            if (context.Segment is IDataPageEvaluatable evaluatable && evaluatable.Evaluator != null)
            {
                await evaluatable.Evaluator.EvaluateAsync(context, cancellationToken);
            }

            return context;
        }

        private ParameterCollection GetTableParameters(DataTable table)
        {
            var parameters = new ParameterCollection();
            foreach (DataColumn column in table.Columns)
            {
                var par = new Parameter(column.ColumnName) { SourceColumn = column.ColumnName };
                par.DbType = column.DataType.GetDbType();
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

            var connection = GetConnection(DistributedMode.Master).TryOpen();

            BeginTransaction();

            using var command = CreateDbCommand(connection, sqlCommand, parameters);
            try
            {
                var result = 0;
                foreach (DataRow row in dataTable.Rows)
                {
                    UpdateParameters(command.Parameters, row);

                    row[COLUMN_RESULT] = command.ExecuteScalar() ?? 0;

                    result++;
                }

                CommitTransaction();

                return result;
            }
            catch (DbException exp)
            {
                throw HandleException(command, exp);
            }
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
        /// <param name="parameters"></param>
        /// <returns></returns>
        private TResult HandleCommandExecuted<TCommand, TResult>(TCommand command, ParameterCollection parameters, CommandBehavior behavior, Func<TCommand, CommandBehavior, TResult> func) where TCommand : IDbCommand
        {
            var tracker = ServiceProvider.TryGetService<ICommandTracker>(() => DefaultCommandTracker.Instance);

            var watch = Stopwatch.StartNew();
            var result = func(command, behavior);
            watch.Stop();

            Tracer.Debug($"The DbCommand was executed ({watch.Elapsed.Milliseconds}ms):\n{command.Output()}");

            if (ConnectionString.IsTracking && tracker != null)
            {
                tracker?.Write(command, watch.Elapsed);
            }

            command.SyncParameters(parameters);
            command.ClearParameters();

            return result;
        }

        /// <summary>
        /// 异步的，通知应用程序，一个 <see cref="DbCommand"/> 已经执行。
        /// </summary>
        /// <param name="command">所执行的 <see cref="DbCommand"/> 对象。</param>
        /// <param name="func">执行的方法。</param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<TResult> HandleCommandExecutedAsync<TCommand, TResult>(TCommand command, ParameterCollection parameters, CommandBehavior behavior, Func<TCommand, CommandBehavior, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken) where TCommand : IDbCommand
        {
            var tracker = ServiceProvider.TryGetService<ICommandTracker>(() => DefaultCommandTracker.Instance);

            var watch = Stopwatch.StartNew();
            var result = await func(command, behavior, cancellationToken).ConfigureAwait(false);
            watch.Stop();
            
            Tracer.Debug($"The DbCommand was executed ({Thread.CurrentThread.ManagedThreadId}th, {watch.Elapsed.Milliseconds}ms):\n{command.Output()}");

            if (ConnectionString.IsTracking && tracker != null)
            {
                await tracker?.WriteAsync(command, watch.Elapsed, cancellationToken);
            }

            command.SyncParameters(parameters);
            command.ClearParameters();

            return result;
        }

        /// <summary>
        /// 处理异常。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="exp"></param>
        private Exception HandleException(IDbCommand command, Exception exp)
        {
            Tracer.Debug($"The DbCommand was throw exception:\n{command.Output()}\n{exp.Output()}");

            var tracker = ServiceProvider.TryGetService<ICommandTracker>(() => DefaultCommandTracker.Instance);

            if (ConnectionString.IsTracking && tracker != null)
            {
                tracker.Fail(command, exp);
            }

            return new CommandException(command, exp);
        }

        /// <summary>
        /// 异步的，处理异常。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="exp"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<Exception> HandleExceptionAsync(IDbCommand command, Exception exp, CancellationToken cancellationToken)
        {
            Tracer.Debug($"The DbCommand was throw exception:\n{command.Output()}\n{exp.Output()}");

            var tracker = ServiceProvider.TryGetService<ICommandTracker>(() => DefaultCommandTracker.Instance);

            if (ConnectionString.IsTracking && tracker != null)
            {
                await tracker.FailAsync(command, exp, cancellationToken);
            }

            return new CommandException(command, exp);
        }

        /// <summary>
        /// 处理使用了 <see cref="IGeneratorProvider"/> 生成数据的数据表。
        /// </summary>
        /// <param name="dataTable"></param>
        private void HandleDynamicDataTable(DataTable dataTable)
        {
            DataColumn genColumn = null;
            DataColumn newColumn = null;
            for (var i = dataTable.Columns.Count - 1; i >= 0; i--)
            {
                var column = dataTable.Columns[i];

                Type parType;
                if ((parType = DataExpressionRow.GetParameterType(column.DataType)) != null)
                {
                    newColumn = dataTable.Columns.Add(column.ColumnName + "_NEW", parType);
                    genColumn = column;
                }
            }

            if (genColumn != null && newColumn != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    var id = (row[genColumn] as DataExpressionRow).GetValue(this);
                    row[genColumn.ColumnName + "_NEW"] = id;
                }

                //移除原来的，新的改名
                dataTable.Columns.Remove(genColumn);
                newColumn.ColumnName = genColumn.ColumnName;
            }
        }

        /// <summary>
        /// 检查是否强制使用主库查询。
        /// </summary>
        /// <returns></returns>
        private DistributedMode CheckForceUseMaster()
        {
            return ForceUseMasterScope.Current != null ? DistributedMode.Master : DistributedMode.Slave;
        }

        /// <summary>
        /// 检查是否强制使用主库查询。
        /// </summary>
        /// <param name="otherwise">返回如果没有使用 <see cref="ForceUseMasterScope"/> 则采用的模式。</param>
        /// <returns></returns>
        private DistributedMode CheckForceUseMaster(Func<DistributedMode> otherwise)
        {
            if (ForceUseMasterScope.Current != null)
            {
                return DistributedMode.Master;
            }

            return otherwise();
        }

        /// <summary>
        /// 调整主从模式。
        /// </summary>
        /// <param name="queryCommand"></param>
        /// <returns></returns>
        private DistributedMode AdjustMode(IQueryCommand queryCommand)
        {
            return ServiceProvider.TryGetService<IDistributedSynchronizer>()?.AdjustMode(this, queryCommand) ?? DistributedMode.Slave;
        }

        /// <summary>
        /// 初始化分布式同步器。
        /// </summary>
        /// <param name="queryCommand"></param>
        private void InitiaizeDistributedSynchronizer(IQueryCommand queryCommand)
        {
            ServiceProvider.TryGetService<IDistributedSynchronizer>()?.CatchExecuting(this, queryCommand);
        }
    }
}