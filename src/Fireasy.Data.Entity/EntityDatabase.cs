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

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 用于在持久化环境中对 <see cref="IDatabase"/> 对象的包装。无法继承此类。
    /// </summary>
    internal sealed class EntityDatabase : IDatabase, IServiceProviderAccessor
    {
        private readonly IDatabase _database;

        internal EntityDatabase(IDatabase database)
        {
            Guard.ArgumentNull(database, nameof(database));
            _database = database;
        }

        void IDisposable.Dispose()
        {
            //如果IDatabase处于持久化环境中，则不销毁，否则会使事务提前回滚
            if (EntityTransactionScope.Current == null)
            {
                _database.Dispose();
            }
        }

        ConnectionString IDatabase.ConnectionString
        {
            get { return _database.ConnectionString; }
            set { _database.ConnectionString = value; }
        }

        IServiceProvider IServiceProviderAccessor.ServiceProvider
        {
            get { return _database.TryGetServiceProvider(); }
            set { _database.TrySetServiceProvider(value); }
        }

        IProvider IDatabase.Provider
        {
            get { return _database.Provider; }
        }

        int IDatabase.Timeout
        {
            get { return _database.Timeout; }
            set { _database.Timeout = value; }
        }

        DbTransaction IDatabase.Transaction
        {
            get { return _database.Transaction; }
        }

        DbConnection IDatabase.Connection
        {
            get { return _database.Connection; }
        }
        bool IDatabase.BeginTransaction(IsolationLevel level)
        {
            return _database.BeginTransaction(level);
        }

        bool IDatabase.CommitTransaction()
        {
            return _database.CommitTransaction();
        }

        bool IDatabase.RollbackTransaction()
        {
            return _database.RollbackTransaction();
        }

        DataTable IDatabase.ExecuteDataTable(IQueryCommand queryCommand, string tableName, IDataSegment segment, ParameterCollection parameters)
        {
            return _database.ExecuteDataTable(queryCommand, tableName, segment, parameters);
        }

        IEnumerable<T> IDatabase.ExecuteEnumerable<T>(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, IDataRowMapper<T> rowMapper)
        {
            return _database.ExecuteEnumerable(queryCommand, segment, parameters, rowMapper);
        }

        IEnumerable<T> IDatabase.ExecuteEnumerable<T>(IQueryCommand queryCommand, Func<IRecordWrapper, IDataReader, T> rowMapper, IDataSegment segment, ParameterCollection parameters)
        {
            return _database.ExecuteEnumerable(queryCommand, rowMapper, segment, parameters);
        }

        IEnumerable<object> IDatabase.ExecuteEnumerable(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters)
        {
            return _database.ExecuteEnumerable(queryCommand, segment, parameters);
        }

        int IDatabase.ExecuteNonQuery(IQueryCommand queryCommand, ParameterCollection parameters)
        {
            return _database.ExecuteNonQuery(queryCommand, parameters);
        }

        int IDatabase.ExecuteBatch(IEnumerable<IQueryCommand> queryCommands, ParameterCollection parameters)
        {
            return _database.ExecuteBatch(queryCommands, parameters);
        }

        IDataReader IDatabase.ExecuteReader(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CommandBehavior? behavior = null)
        {
            return _database.ExecuteReader(queryCommand, segment, parameters, behavior);
        }

        object IDatabase.ExecuteScalar(IQueryCommand queryCommand, ParameterCollection parameters)
        {
            return _database.ExecuteScalar(queryCommand, parameters);
        }

        T IDatabase.ExecuteScalar<T>(IQueryCommand queryCommand, ParameterCollection parameters)
        {
            return _database.ExecuteScalar<T>(queryCommand, parameters);
        }

        void IDatabase.FillDataSet(DataSet dataSet, IQueryCommand queryCommand, string tableName, IDataSegment segment, ParameterCollection parameters)
        {
            _database.FillDataSet(dataSet, queryCommand, tableName, segment, parameters);
        }

        void IDatabase.Update(DataTable dataTable)
        {
            _database.Update(dataTable);
        }

        int IDatabase.Update(DataTable dataTable, SqlCommand insertCommand, SqlCommand updateCommand, SqlCommand deleteCommand)
        {
            return _database.Update(dataTable, insertCommand, updateCommand, deleteCommand);
        }

        Task<int> IDatabase.ExecuteNonQueryAsync(IQueryCommand queryCommand, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _database.ExecuteNonQueryAsync(queryCommand, parameters, cancellationToken);
        }

        Task<int> IDatabase.ExecuteBatchAsync(IEnumerable<IQueryCommand> queryCommands, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _database.ExecuteBatchAsync(queryCommands, parameters, cancellationToken);
        }

        Task<IDataReader> IDatabase.ExecuteReaderAsync(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CommandBehavior? behavior, CancellationToken cancellationToken)
        {
            return _database.ExecuteReaderAsync(queryCommand, segment, parameters, behavior, cancellationToken);
        }

        Task<object> IDatabase.ExecuteScalarAsync(IQueryCommand queryCommand, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _database.ExecuteScalarAsync(queryCommand, parameters, cancellationToken);
        }

        Task<T> IDatabase.ExecuteScalarAsync<T>(IQueryCommand queryCommand, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _database.ExecuteScalarAsync<T>(queryCommand, parameters, cancellationToken);
        }

#if NETSTANDARD2_1_OR_GREATER
        IAsyncEnumerable<T> IDatabase.ExecuteAsyncEnumerable<T>(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, IDataRowMapper<T> rowMapper, CancellationToken cancellationToken)
        {
            return _database.ExecuteAsyncEnumerable<T>(queryCommand, segment, parameters, rowMapper, cancellationToken);
        }

        IAsyncEnumerable<dynamic> IDatabase.ExecuteAsyncEnumerable(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _database.ExecuteAsyncEnumerable(queryCommand, segment, parameters, cancellationToken);
        }
#endif
        Task<IEnumerable<T>> IDatabase.ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, IDataRowMapper<T> rowMapper, CancellationToken cancellationToken)
        {
            return _database.ExecuteEnumerableAsync<T>(queryCommand, segment, parameters, rowMapper, cancellationToken);
        }

        Task<IEnumerable<T>> IDatabase.ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, Func<IRecordWrapper, IDataReader, T> rowMapper, IDataSegment segment, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _database.ExecuteEnumerableAsync(queryCommand, rowMapper, segment, parameters, cancellationToken);
        }

        Task<IEnumerable<dynamic>> IDatabase.ExecuteEnumerableAsync(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return _database.ExecuteEnumerableAsync(queryCommand, segment, parameters, cancellationToken);
        }

        Exception IDatabase.TryConnect()
        {
            return _database.TryConnect();
        }

        Task IDatabase.UpdateAsync(DataTable dataTable, CancellationToken cancellationToken)
        {
            return _database.UpdateAsync(dataTable, cancellationToken);
        }

        Task<int> IDatabase.UpdateAsync(DataTable dataTable, SqlCommand insertCommand, SqlCommand updateCommand, SqlCommand deleteCommand, CancellationToken cancellationToken)
        {
            return _database.UpdateAsync(dataTable, insertCommand, updateCommand, deleteCommand, cancellationToken);
        }

        Task<Exception> IDatabase.TryConnectAsync(CancellationToken cancellationToken)
        {
            return _database.TryConnectAsync(cancellationToken);
        }
    }
}