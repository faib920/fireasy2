// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Provider;
using Fireasy.Data.RecordWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data
{
    /// <summary>
    /// 提供一个 <see cref="IDatabase"/> 的包装，在 Dispose 时不调用实际的销毁方法。
    /// </summary>
    public sealed class NoDisposeDatabase : IDatabase, IServiceProviderAccessor
    {
        private readonly IDatabase _innerDatabase;

        /// <summary>
        /// 初始化 <see cref="NoDisposeDatabase"/> 类的新实例。
        /// </summary>
        /// <param name="database">受包装的 <see cref="IDatabase"/> 对象。</param>
        public NoDisposeDatabase(IDatabase database)
        {
            Guard.ArgumentNull(database, nameof(database));
            _innerDatabase = database;
        }

        ConnectionString IDatabase.ConnectionString
        {
            get { return _innerDatabase.ConnectionString; }
            set { _innerDatabase.ConnectionString = value; }
        }

        IServiceProvider IServiceProviderAccessor.ServiceProvider
        {
            get { return _innerDatabase.TryGetServiceProvider(); }
            set { _innerDatabase.TrySetServiceProvider(value); }
        }

        IProvider IDatabase.Provider
        {
            get { return _innerDatabase.Provider; }
        }

        int IDatabase.Timeout
        {
            get { return _innerDatabase.Timeout; }
            set { _innerDatabase.Timeout = value; }
        }

        DbTransaction IDatabase.Transaction
        {
            get { return _innerDatabase.Transaction; }
        }

        DbConnection IDatabase.Connection
        {
            get { return _innerDatabase.Connection; }
        }

        bool IDatabase.BeginTransaction(IsolationLevel level)
        {
            return _innerDatabase.BeginTransaction(level);
        }

        bool IDatabase.CommitTransaction()
        {
            return _innerDatabase.CommitTransaction();
        }

        bool IDatabase.RollbackTransaction()
        {
            return _innerDatabase.RollbackTransaction();
        }

        DataTable IDatabase.ExecuteDataTable(IQueryCommand queryCommand, string tableName, IDataSegment segment, ParameterCollection parameters)
        {
            return _innerDatabase.ExecuteDataTable(queryCommand, tableName, segment, parameters);
        }

        IEnumerable<T> IDatabase.ExecuteEnumerable<T>(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, IDataRowMapper<T> rowMapper)
        {
            return _innerDatabase.ExecuteEnumerable(queryCommand, segment, parameters, rowMapper);
        }

        IEnumerable<T> IDatabase.ExecuteEnumerable<T>(IQueryCommand queryCommand, Func<IRecordWrapper, IDataReader, T> rowMapper, IDataSegment segment, ParameterCollection parameters)
        {
            return _innerDatabase.ExecuteEnumerable(queryCommand, rowMapper, segment, parameters);
        }

        IEnumerable<object> IDatabase.ExecuteEnumerable(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters)
        {
            return _innerDatabase.ExecuteEnumerable(queryCommand, segment, parameters);
        }

        int IDatabase.ExecuteNonQuery(IQueryCommand queryCommand, ParameterCollection parameters)
        {
            return _innerDatabase.ExecuteNonQuery(queryCommand, parameters);
        }

        int IDatabase.ExecuteBatch(IEnumerable<IQueryCommand> queryCommands, ParameterCollection parameters)
        {
            return _innerDatabase.ExecuteBatch(queryCommands, parameters);
        }

        IDataReader IDatabase.ExecuteReader(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CommandBehavior? behavior)
        {
            return _innerDatabase.ExecuteReader(queryCommand, segment, parameters, behavior);
        }

        object IDatabase.ExecuteScalar(IQueryCommand queryCommand, ParameterCollection parameters)
        {
            return _innerDatabase.ExecuteScalar(queryCommand, parameters);
        }

        T IDatabase.ExecuteScalar<T>(IQueryCommand queryCommand, ParameterCollection parameters)
        {
            return _innerDatabase.ExecuteScalar<T>(queryCommand, parameters);
        }

        void IDatabase.FillDataSet(DataSet dataSet, IQueryCommand queryCommand, string tableName, IDataSegment segment, ParameterCollection parameters)
        {
            _innerDatabase.FillDataSet(dataSet, queryCommand, tableName, segment, parameters);
        }

        void IDatabase.Update(DataTable dataTable)
        {
            _innerDatabase.Update(dataTable);
        }

        int IDatabase.Update(DataTable dataTable, SqlCommand insertCommand, SqlCommand updateCommand, SqlCommand deleteCommand)
        {
            return _innerDatabase.Update(dataTable, insertCommand, updateCommand, deleteCommand);
        }

        Task<int> IDatabase.ExecuteNonQueryAsync(IQueryCommand queryCommand, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _innerDatabase.ExecuteNonQueryAsync(queryCommand, parameters, cancellationToken);
        }

        Task<int> IDatabase.ExecuteBatchAsync(IEnumerable<IQueryCommand> queryCommands, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _innerDatabase.ExecuteBatchAsync(queryCommands, parameters, cancellationToken);
        }

        Task<IDataReader> IDatabase.ExecuteReaderAsync(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CommandBehavior? behavior, CancellationToken cancellationToken)
        {
            return _innerDatabase.ExecuteReaderAsync(queryCommand, segment, parameters, behavior, cancellationToken);
        }

        Task<object> IDatabase.ExecuteScalarAsync(IQueryCommand queryCommand, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _innerDatabase.ExecuteScalarAsync(queryCommand, parameters, cancellationToken);
        }

        Task<T> IDatabase.ExecuteScalarAsync<T>(IQueryCommand queryCommand, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _innerDatabase.ExecuteScalarAsync<T>(queryCommand, parameters, cancellationToken);
        }

#if NETSTANDARD && !NETSTANDARD2_0
        IAsyncEnumerable<T> IDatabase.ExecuteAsyncEnumerable<T>(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, IDataRowMapper<T> rowMapper, CancellationToken cancellationToken)
        {
            return _innerDatabase.ExecuteAsyncEnumerable<T>(queryCommand, segment, parameters, rowMapper, cancellationToken);
        }

        IAsyncEnumerable<dynamic> IDatabase.ExecuteAsyncEnumerable(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _innerDatabase.ExecuteAsyncEnumerable(queryCommand, segment, parameters, cancellationToken);
        }
#endif
        Task<IEnumerable<T>> IDatabase.ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, IDataRowMapper<T> rowMapper, CancellationToken cancellationToken)
        {
            return _innerDatabase.ExecuteEnumerableAsync<T>(queryCommand, segment, parameters, rowMapper, cancellationToken);
        }

        Task<IEnumerable<T>> IDatabase.ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, Func<IRecordWrapper, IDataReader, T> rowMapper, IDataSegment segment, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _innerDatabase.ExecuteEnumerableAsync(queryCommand, rowMapper, segment, parameters, cancellationToken);
        }

        Task<IEnumerable<dynamic>> IDatabase.ExecuteEnumerableAsync(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _innerDatabase.ExecuteEnumerableAsync(queryCommand, segment, parameters, cancellationToken);
        }

        Task IDatabase.UpdateAsync(DataTable dataTable, CancellationToken cancellationToken)
        {
            return _innerDatabase.UpdateAsync(dataTable, cancellationToken);
        }

        Task<int> IDatabase.UpdateAsync(DataTable dataTable, SqlCommand insertCommand, SqlCommand updateCommand, SqlCommand deleteCommand, CancellationToken cancellationToken)
        {
            return _innerDatabase.UpdateAsync(dataTable, insertCommand, updateCommand, deleteCommand, cancellationToken);
        }

        Task<Exception> IDatabase.TryConnectAsync(CancellationToken cancellationToken)
        {
            return _innerDatabase.TryConnectAsync(cancellationToken);
        }

        Exception IDatabase.TryConnect()
        {
            return _innerDatabase.TryConnect();
        }

        void IDisposable.Dispose()
        {
        }
    }
}