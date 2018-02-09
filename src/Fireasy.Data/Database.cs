// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using Fireasy.Data.Provider;
using Fireasy.Data.RecordWrapper;
using Fireasy.Data.Syntax;
#if !NET35
using System.Dynamic;
using Fireasy.Common.ComponentModel;
#endif
#if !NET35 && !NET40
using System.Threading.Tasks;
#endif

namespace Fireasy.Data
{
    /// <summary>
    /// 提供数据库基本操作的方法。
    /// </summary>
    public class Database : IDatabase
    {
        private readonly TransactionStack tranStack;
        private bool isDisposed;
        private readonly DatabaseScope dbScope;
#if !NET35 && !NET40
        private readonly AsyncTaskManager taskMgr;
#endif
        private DbConnection connection;

        /// <summary>
        /// 初始化 <see cref="Database"/> 类的新实例。
        /// </summary>
        protected Database()
        {
            tranStack = new TransactionStack();
            dbScope = new DatabaseScope(this);
#if !NET35 && !NET40
            taskMgr = new AsyncTaskManager(this);
#endif
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
            Track = DefaultCommandTracker.Instance;
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
                if (connection == null)
                {
                    if (Transaction != null)
                    {
                        connection = Transaction.Connection;
                    }
                    else
                    {
                        connection = TransactionScopeConnections.GetConnection(this);
                        if (connection == null)
                        {
                            connection = this.CreateConnection();
                            connection.StateChange += (o, e) => OnConnectionStateChanged(e.OriginalState, e.CurrentState);
                        }
                    }
                }

                return connection;
            }
            private set
            {
                connection = value;
            }
        }

        /// <summary>
        /// 使用指定锁定行为启动一个数据库事务。
        /// </summary>
        /// <param name="level">事务的锁定行为。</param>
        /// <returns>如果当前实例首次启动事务，则为 true，否则为 false。</returns>
        public virtual bool BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
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

#if !NET35 && !NET40
            if (taskMgr.HasTasks)
            {
                return false;
            }
#endif

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

#if !NET35 && !NET40
            if (taskMgr.HasTasks)
            {
                return false;
            }
#endif

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
                    yield return rowMapper.Map(reader);
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
        public virtual IEnumerable<object> ExecuteEnumerable(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));

            using (var reader = ExecuteReader(queryCommand, segment, parameters))
            {
                var wrapper = Provider.GetService<IRecordWrapper>();
#if !NET35
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
#else
                var builder = new DataReaderTypeBuilder(reader);
                var elementType = builder.CreateType();
                while (reader.Read())
                {
                    yield return elementType.New(reader, wrapper);
                }
#endif
            }
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
            return UsingConnection(() =>
                {
                    using (var command = CreateDbCommand(queryCommand, parameters))
                    {
                        try
                        {
                            var result = HandleCommandExecute(command, () => command.ExecuteNonQuery());

                            command.SyncParameters(parameters);
                            command.ClearParameters();

                            return result;
                        }
                        catch (DbException exp)
                        {
                            HandleFailedLog(command, exp);

                            throw new CommandException(command, exp);
                        }
                    }
                });
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
            return UsingConnection(() =>
                {
                    var command = CreateDbCommand(queryCommand, parameters);
                    try
                    {
                        var cmdBehavior = CommandBehavior.Default;
                        var context = new CommandContext(this, command, segment, parameters);
                        HandleSegmentCommand(context);

                        IDataReader result = HandleCommandExecute(command, () => command.ExecuteReader(cmdBehavior));

                        command.SyncParameters(parameters);
                        command.ClearParameters();

                        return result;
                    }
                    catch (DbException exp)
                    {
                        HandleFailedLog(command, exp);

                        throw new CommandException(command, exp);
                    }
                });
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
            return UsingConnection(() =>
                {
                    using (var command = CreateDbCommand(queryCommand, parameters))
                    {
                        try
                        {
                            var result = HandleCommandExecute(command, () => command.ExecuteScalar());

                            command.SyncParameters(parameters);
                            command.ClearParameters();
                            return result;
                        }
                        catch (DbException exp)
                        {
                            HandleFailedLog(command, exp);

                            throw new CommandException(command, exp);
                        }
                    }
                });
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
            Guard.NullReference(adapter);

            UsingConnection(() =>
                {
                    using (var command = CreateDbCommand(queryCommand, parameters))
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
                            var handler = !HandleSegmentCommand(context) ?
                                new Func<int>(() => adapter.Fill(dataSet, segment.Start.Value, segment.Length, "Table")) :
                                new Func<int>(() => adapter.Fill(dataSet));

                            HandleCommandExecute(command, handler);
                        }
                        catch (DbException exp)
                        {
                            HandleFailedLog(command, exp);

                            throw new CommandException(command, exp);
                        }
                    }
                }, false);
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
        /// 将 <see cref="DataTable"/> 的更改保存到数据库中，这类更改包括新增、修改和删除的数据。
        /// </summary>
        /// <param name="dataTable">要更新的数据表对象。</param>
        public void Update(DataTable dataTable)
        {
            Guard.ArgumentNull(dataTable, nameof(dataTable));

            UsingConnection(() =>
                {
                    var builder = new CommandBuilder(Provider, dataTable, Connection, Transaction);
                    var adapter = Provider.DbProviderFactory.CreateDataAdapter();
                    Guard.NullReference(adapter);
                    builder.FillAdapter(adapter);
                    adapter.Update(dataTable);
                }, false);
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
            var result = -1;

            UsingConnection(() =>
                {
                    var parameters = GetTableParameters(dataTable);
                    var adapter = Provider.DbProviderFactory.CreateDataAdapter();
                    if (insertCommand != null)
                    {
                        adapter.InsertCommand = CreateDbCommand(insertCommand, parameters);
                        adapter.InsertCommand.UpdatedRowSource = UpdateRowSource.Both;
                    }

                    var period = TimeWatcher.Watch(() => result = adapter.Update(dataTable));
                    HandleLog(adapter.InsertCommand, period);

                }, false);

            return result;
        }

#if !NET40 && !NET35
        /// <summary>
        /// 异步执行查询文本，返回受影响的记录数。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>所影响的记录数。</returns>
        public async virtual Task<int> ExecuteNonQueryAsync(IQueryCommand queryCommand, ParameterCollection parameters = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));
            return await UsingConnection(() =>
                {
                    var command = CreateDbCommand(queryCommand, parameters);
                    try
                    {
                        return HandleExecuteTask(HandleCommandExecute(command, () => command.ExecuteNonQueryAsync()), command, parameters);
                    }
                    catch (DbException exp)
                    {
                        HandleFailedLog(command, exp);
                        command.Dispose();

                        throw new CommandException(command, exp);
                    }
                });
        }

        /// <summary>
        /// 异步执行查询文本并返回一个 <see cref="IDataReader"/>。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>一个 <see cref="IDataReader"/> 对象。</returns>
        public async virtual Task<IDataReader> ExecuteReaderAsync(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));
            return await UsingConnection(() =>
                {
                    var command = CreateDbCommand(queryCommand, parameters);
                    try
                    {
                        var context = new CommandContext(this, command, segment, parameters);
                        HandleSegmentCommand(context);

                        return HandleExecuteTask(HandleCommandExecute(command, () => command.ExecuteReaderAsync()), command, parameters);
                    }
                    catch (DbException exp)
                    {
                        HandleFailedLog(command, exp);
                        command.Dispose();

                        throw new CommandException(command, exp);
                    }
                });
        }

        /// <summary>
        /// 异步执行查询文本，并返回第一行的第一列。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>第一行的第一列数据。</returns>
        public async virtual Task<object> ExecuteScalarAsync(IQueryCommand queryCommand, ParameterCollection parameters = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));
            return await UsingConnection(() =>
                {
                    var command = CreateDbCommand(queryCommand, parameters);
                    try
                    {
                        return (HandleExecuteTask(HandleCommandExecute(command, () => command.ExecuteScalarAsync()), command, parameters));
                    }
                    catch (DbException exp)
                    {
                        HandleFailedLog(command, exp);
                        command.Dispose();

                        throw new CommandException(command, exp);
                    }
                });
        }

        /// <summary>
        /// 异步执行查询文本，并返回第一行的第一列。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>第一行的第一列数据。</returns>
        public async virtual Task<T> ExecuteScalarAsync<T>(IQueryCommand queryCommand, ParameterCollection parameters = null)
        {
            var result = await ExecuteScalarAsync(queryCommand, parameters);

            return result == DBNull.Value ? default(T) : result.To<T>();
        }

        /// <summary>
        /// 异步执行查询文本并将结果以一个 <see cref="IEnumerable{T}"/> 的序列返回。
        /// </summary>
        /// <typeparam name="T">查询对象类型。</typeparam>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="rowMapper">数据行映射器。</param>
        /// <returns>一个 <typeparamref name="T"/> 类型的对象的枚举器。</returns>
        public async virtual Task<IEnumerable<T>> ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, IDataRowMapper<T> rowMapper = null)
        {
            Guard.ArgumentNull(queryCommand, nameof(queryCommand));

            rowMapper = rowMapper ?? RowMapperFactory.CreateRowMapper<T>();
            rowMapper.RecordWrapper = Provider.GetService<IRecordWrapper>();

            var reader = await ExecuteReaderAsync(queryCommand, segment, parameters);
            return new AsyncEnumerable<T>(reader, rowMapper);
        }
#endif

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
#if !NET35 && !NET40
            if (taskMgr.HasTasks)
            {
                return;
            }
#endif
            if (isDisposed)
            {
                return;
            }

            if (Transaction != null)
            {
                Transaction.Dispose();
                Transaction = null;
            }

            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }

            dbScope.Dispose();
#if !NET35 && !NET40
            taskMgr.Dispose();
#endif
            isDisposed = true;
        }

        /// <summary>
        /// 通知应用程序，一个 <see cref="DbCommand"/> 已经执行。
        /// </summary>
        /// <param name="command">所执行的 <see cref="IDbCommand"/> 对象。</param>
        /// <param name="func">执行的方法。</param>
        protected virtual T HandleCommandExecute<T>(IDbCommand command, Func<T> func)
        {
            var result = default(T);
            var period = TimeWatcher.Watch(() => result = func());
            Debug.WriteLine("Execute: " + command.Output() + "\nDissipation times: " + period);

            HandleLog(command, period);

            return result;
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
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">参数集合。</param>
        /// <returns>一个由 <see cref="IQueryCommand"/> 和参数集合组成的 <see cref="DbCommand"/> 对象。</returns>
        private DbCommand CreateDbCommand(IQueryCommand queryCommand, IEnumerable<Parameter> parameters)
        {
            var command = Connection.CreateCommand();
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

            return command;
        }

        private T UsingConnection<T>(Func<T> callback, bool isAutoOpen = true)
        {
            var result = default(T);
            if (callback != null)
            {
                Connection.TryOpen(isAutoOpen);
                result = callback();
            }

            return result;
        }

        private void UsingConnection(Action callback, bool isAutoOpen = true)
        {
            if (callback != null)
            {
                Connection.TryOpen(isAutoOpen);
                callback();
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
        private void HandleFailedLog(DbCommand command, Exception exp)
        {
            if (ConnectionString.IsTracking && Track != null)
            {
                Track.Fail(command, exp);
            }
        }

        /// <summary>
        /// 处理日志。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="period"></param>
        private void HandleLog(IDbCommand command, TimeSpan period)
        {
            if (Log != null)
            {
                Log(command, period);
            }
            else if (ConnectionString.IsTracking && Track != null)
            {
                Track.Write(command, period);
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

#if !NET40 && !NET35
        private class AsyncEnumerable<T> : IEnumerable<T>
        {
            private IEnumerator<T> enumerator;

            public AsyncEnumerable(IDataReader reader, IDataRowMapper<T> rowMapper)
            {
                this.enumerator = new AsyncEnumerator<T>(reader, rowMapper);
            }


            public IEnumerator<T> GetEnumerator()
            {
                return enumerator;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return enumerator;
            }
        }

        private class AsyncEnumerator<T> : IEnumerator<T>
        {
            private IDataReader reader;
            private IDataRowMapper<T> rowMapper;

            public AsyncEnumerator(IDataReader reader, IDataRowMapper<T> rowMapper)
            {
                this.reader = reader;
                this.rowMapper = rowMapper;
            }

            public T Current
            {
                get { return rowMapper.Map(reader); }
            }

            public void Dispose()
            {
                reader.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                return reader.Read();
            }

            public void Reset()
            {
            }
        }

        /// <summary>
        /// 执行完后对 <see cref="DbCommand"/> 进行参数同步、清理和销毁处理。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private Task<T> HandleExecuteTask<T>(Task<T> task, DbCommand command, ParameterCollection parameters)
        {
            return task.ContinueWith(t =>
                {
                    command.SyncParameters(parameters);
                    command.ClearParameters();
                    command.Dispose();

                    return t.Result;
                });
        }
#endif
    }
}