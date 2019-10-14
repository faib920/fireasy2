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
using Fireasy.Common;
using Fireasy.Data.Provider;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 用于在持久化环境中对 <see cref="IDatabase"/> 对象的包装。无法继承此类。
    /// </summary>
    internal sealed class EntityDatabase : IDatabase
    {
        private readonly IDatabase database;

        internal EntityDatabase(IDatabase database)
        {
            Guard.ArgumentNull(database, nameof(database));
            this.database = database;
        }

        void IDisposable.Dispose()
        {
            //如果IDatabase处于持久化环境中，则不销毁，否则会使事务提前回滚
            if (EntityTransactionScope.Current == null)
            {
                database.Dispose();
            }
        }

        ConnectionString IDatabase.ConnectionString
        {
            get { return database.ConnectionString; }
            set { database.ConnectionString = value; }
        }

        IProvider IDatabase.Provider
        {
            get { return database.Provider; }
        }

        int IDatabase.Timeout
        {
            get { return database.Timeout; }
            set { database.Timeout = value; }
        }

        DbTransaction IDatabase.Transaction
        {
            get { return database.Transaction; }
        }

        DbConnection IDatabase.Connection
        {
            get { return database.Connection; }
        }

        Action<IDbCommand, TimeSpan> IDatabase.Log
        {
            get { return database.Log; }
            set { database.Log = value; }
        }

        bool IDatabase.BeginTransaction(IsolationLevel level)
        {
            return database.BeginTransaction(level);
        }

        bool IDatabase.CommitTransaction()
        {
            return database.CommitTransaction();
        }

        bool IDatabase.RollbackTransaction()
        {
            return database.RollbackTransaction();
        }

        DataTable IDatabase.ExecuteDataTable(IQueryCommand queryCommand, string tableName, IDataSegment segment, ParameterCollection parameters)
        {
            return database.ExecuteDataTable(queryCommand, tableName, segment, parameters);
        }

        IEnumerable<T> IDatabase.ExecuteEnumerable<T>(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, IDataRowMapper<T> rowMapper)
        {
            return database.ExecuteEnumerable(queryCommand, segment, parameters, rowMapper);
        }

        IEnumerable<object> IDatabase.ExecuteEnumerable(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters)
        {
            return database.ExecuteEnumerable(queryCommand, segment, parameters);
        }

        int IDatabase.ExecuteNonQuery(IQueryCommand queryCommand, ParameterCollection parameters)
        {
            return database.ExecuteNonQuery(queryCommand, parameters);
        }

        IDataReader IDatabase.ExecuteReader(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters)
        {
            return database.ExecuteReader(queryCommand, segment, parameters);
        }

        object IDatabase.ExecuteScalar(IQueryCommand queryCommand, ParameterCollection parameters)
        {
            return database.ExecuteScalar(queryCommand, parameters);
        }

        T IDatabase.ExecuteScalar<T>(IQueryCommand queryCommand, ParameterCollection parameters)
        {
            return database.ExecuteScalar<T>(queryCommand, parameters);
        }

        void IDatabase.FillDataSet(DataSet dataSet, IQueryCommand queryCommand, string tableName, IDataSegment segment, ParameterCollection parameters)
        {
            database.FillDataSet(dataSet, queryCommand, tableName, segment, parameters);
        }

        void IDatabase.Update(DataTable dataTable)
        {
            database.Update(dataTable);
        }

        int IDatabase.Update(DataTable dataTable, SqlCommand insertCommand, SqlCommand updateCommand, SqlCommand deleteCommand)
        {
            return database.Update(dataTable, insertCommand, updateCommand, deleteCommand);
        }

        Task<int> IDatabase.ExecuteNonQueryAsync(IQueryCommand queryCommand, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return database.ExecuteNonQueryAsync(queryCommand, parameters, cancellationToken);
        }

        Task<IDataReader> IDatabase.ExecuteReaderAsync(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return database.ExecuteReaderAsync(queryCommand, segment, parameters, cancellationToken);
        }

        Task<object> IDatabase.ExecuteScalarAsync(IQueryCommand queryCommand, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return database.ExecuteScalarAsync(queryCommand, parameters, cancellationToken);
        }

        Task<T> IDatabase.ExecuteScalarAsync<T>(IQueryCommand queryCommand, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return database.ExecuteScalarAsync<T>(queryCommand, parameters, cancellationToken);
        }

#if NETSTANDARD && !NETSTANDARD2_0
        IAsyncEnumerable<T> IDatabase.ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, IDataRowMapper<T> rowMapper, CancellationToken cancellationToken)
        {
            return database.ExecuteEnumerableAsync<T>(queryCommand, segment, parameters, rowMapper, cancellationToken);
        }

        IAsyncEnumerable<dynamic> IDatabase.ExecuteEnumerableAsync(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return database.ExecuteEnumerableAsync(queryCommand, segment, parameters, cancellationToken);
        }
#else
        Task<IEnumerable<T>> IDatabase.ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, IDataRowMapper<T> rowMapper, CancellationToken cancellationToken)
        {
            return database.ExecuteEnumerableAsync<T>(queryCommand, segment, parameters, rowMapper, cancellationToken);
        }

        Task<IEnumerable<dynamic>> IDatabase.ExecuteEnumerableAsync(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return database.ExecuteEnumerableAsync(queryCommand, segment, parameters, cancellationToken);
        }
#endif

        Exception IDatabase.TryConnect()
        {
            return database.TryConnect();
        }

        Task IDatabase.UpdateAsync(DataTable dataTable, CancellationToken cancellationToken)
        {
            return database.UpdateAsync(dataTable, cancellationToken);
        }

        Task<int> IDatabase.UpdateAsync(DataTable dataTable, SqlCommand insertCommand, SqlCommand updateCommand, SqlCommand deleteCommand, CancellationToken cancellationToken)
        {
            return database.UpdateAsync(dataTable, insertCommand, updateCommand, deleteCommand, cancellationToken);
        }

        Task<Exception> IDatabase.TryConnectAsync(CancellationToken cancellationToken)
        {
            return database.TryConnectAsync(cancellationToken);
        }
    }
}