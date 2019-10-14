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
namespace Fireasy.Data
{
    /// <summary>
    /// 提供一个 <see cref="IDatabase"/> 的包装，在 Dispose 时不调用实际的销毁方法。
    /// </summary>
    public sealed class NoDisposeDatabase : IDatabase
    {
        private readonly IDatabase innerDatabase;

        /// <summary>
        /// 初始化 <see cref="NoDisposeDatabase"/> 类的新实例。
        /// </summary>
        /// <param name="database">受包装的 <see cref="IDatabase"/> 对象。</param>
        public NoDisposeDatabase(IDatabase database)
        {
            Guard.ArgumentNull(database, nameof(database));
            innerDatabase = database;
        }

        ConnectionString IDatabase.ConnectionString
        {
            get { return innerDatabase.ConnectionString; }
            set { innerDatabase.ConnectionString = value; }
        }

        IProvider IDatabase.Provider
        {
            get { return innerDatabase.Provider; }
        }

        int IDatabase.Timeout
        {
            get { return innerDatabase.Timeout; }
            set { innerDatabase.Timeout = value; }
        }

        DbTransaction IDatabase.Transaction
        {
            get { return innerDatabase.Transaction; }
        }

        DbConnection IDatabase.Connection
        {
            get { return innerDatabase.Connection; }
        }

        Action<IDbCommand, TimeSpan> IDatabase.Log
        {
            get { return innerDatabase.Log; }
            set { innerDatabase.Log = value; }
        }

        bool IDatabase.BeginTransaction(IsolationLevel level)
        {
            return innerDatabase.BeginTransaction(level);
        }

        bool IDatabase.CommitTransaction()
        {
            return innerDatabase.CommitTransaction();
        }

        bool IDatabase.RollbackTransaction()
        {
            return innerDatabase.RollbackTransaction();
        }

        DataTable IDatabase.ExecuteDataTable(IQueryCommand queryCommand, string tableName, IDataSegment segment, ParameterCollection parameters)
        {
            return innerDatabase.ExecuteDataTable(queryCommand, tableName, segment, parameters);
        }

        IEnumerable<T> IDatabase.ExecuteEnumerable<T>(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, IDataRowMapper<T> rowMapper)
        {
            return innerDatabase.ExecuteEnumerable(queryCommand, segment, parameters, rowMapper);
        }

        IEnumerable<object> IDatabase.ExecuteEnumerable(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters)
        {
            return innerDatabase.ExecuteEnumerable(queryCommand, segment, parameters);
        }

        int IDatabase.ExecuteNonQuery(IQueryCommand queryCommand, ParameterCollection parameters)
        {
            return innerDatabase.ExecuteNonQuery(queryCommand, parameters);
        }

        IDataReader IDatabase.ExecuteReader(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters)
        {
            return innerDatabase.ExecuteReader(queryCommand, segment, parameters);
        }

        object IDatabase.ExecuteScalar(IQueryCommand queryCommand, ParameterCollection parameters)
        {
            return innerDatabase.ExecuteScalar(queryCommand, parameters);
        }

        T IDatabase.ExecuteScalar<T>(IQueryCommand queryCommand, ParameterCollection parameters)
        {
            return innerDatabase.ExecuteScalar<T>(queryCommand, parameters);
        }

        void IDatabase.FillDataSet(DataSet dataSet, IQueryCommand queryCommand, string tableName, IDataSegment segment, ParameterCollection parameters)
        {
            innerDatabase.FillDataSet(dataSet, queryCommand, tableName, segment, parameters);
        }

        void IDatabase.Update(DataTable dataTable)
        {
            innerDatabase.Update(dataTable);
        }

        int IDatabase.Update(DataTable dataTable, SqlCommand insertCommand, SqlCommand updateCommand, SqlCommand deleteCommand)
        {
            return innerDatabase.Update(dataTable, insertCommand, updateCommand, deleteCommand);
        }

        Task<int> IDatabase.ExecuteNonQueryAsync(IQueryCommand queryCommand, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return innerDatabase.ExecuteNonQueryAsync(queryCommand, parameters, cancellationToken);
        }

        Task<IDataReader> IDatabase.ExecuteReaderAsync(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return innerDatabase.ExecuteReaderAsync(queryCommand, segment, parameters, cancellationToken);
        }

        Task<object> IDatabase.ExecuteScalarAsync(IQueryCommand queryCommand, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return innerDatabase.ExecuteScalarAsync(queryCommand, parameters, cancellationToken);
        }

        Task<T> IDatabase.ExecuteScalarAsync<T>(IQueryCommand queryCommand, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return innerDatabase.ExecuteScalarAsync<T>(queryCommand, parameters, cancellationToken);
        }

#if NETSTANDARD && !NETSTANDARD2_0
        IAsyncEnumerable<T> IDatabase.ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, IDataRowMapper<T> rowMapper, CancellationToken cancellationToken)
        {
            return innerDatabase.ExecuteEnumerableAsync<T>(queryCommand, segment, parameters, rowMapper, cancellationToken);
        }

        IAsyncEnumerable<dynamic> IDatabase.ExecuteEnumerableAsync(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return innerDatabase.ExecuteEnumerableAsync(queryCommand, segment, parameters, cancellationToken);
        }
#else
        Task<IEnumerable<T>> IDatabase.ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, IDataRowMapper<T> rowMapper, CancellationToken cancellationToken)
        {
            return innerDatabase.ExecuteEnumerableAsync<T>(queryCommand, segment, parameters, rowMapper, cancellationToken);
        }

        Task<IEnumerable<dynamic>> IDatabase.ExecuteEnumerableAsync(IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters, CancellationToken cancellationToken)
        {
            return innerDatabase.ExecuteEnumerableAsync(queryCommand, segment, parameters, cancellationToken);
        }
#endif

        Task IDatabase.UpdateAsync(DataTable dataTable, CancellationToken cancellationToken)
        {
            return innerDatabase.UpdateAsync(dataTable, cancellationToken);
        }

        Task<int> IDatabase.UpdateAsync(DataTable dataTable, SqlCommand insertCommand, SqlCommand updateCommand, SqlCommand deleteCommand, CancellationToken cancellationToken)
        {
            return innerDatabase.UpdateAsync(dataTable, insertCommand, updateCommand, deleteCommand, cancellationToken);
        }

        Task<Exception> IDatabase.TryConnectAsync(CancellationToken cancellationToken)
        {
            return innerDatabase.TryConnectAsync( cancellationToken);
        }

        Exception IDatabase.TryConnect()
        {
            return innerDatabase.TryConnect();
        }

        void IDisposable.Dispose()
        {
        }
    }
}