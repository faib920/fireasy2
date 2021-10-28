// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Provider;
using Microsoft.Data.SqlClient;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.MsSqlClient
{
    public class SqlBulkCopyProvider : Batcher.IBulkCopyProvider
    {
        private SqlBulkCopy _bulkCopy;

        IProvider IProviderService.Provider { get; set; }

        void IDisposable.Dispose()
        {
            _bulkCopy.TryDispose();
        }

        public void Initialize(DbConnection connection, DbTransaction transaction, string tableName, int batchSize)
        {
            _bulkCopy = new SqlBulkCopy((SqlConnection)connection, SqlBulkCopyOptions.KeepIdentity, (SqlTransaction)transaction)
            {
                DestinationTableName = tableName,
                BatchSize = batchSize
            };
        }

        public void AddColumnMapping(int sourceColumnIndex, string destinationColumn)
        {
            _bulkCopy.ColumnMappings.Add(sourceColumnIndex, destinationColumn);
        }

        public void WriteToServer(DbDataReader reader)
        {
            _bulkCopy.WriteToServer(reader);
        }

        public async Task WriteToServerAsync(DbDataReader reader, CancellationToken cancellationToken)
        {
            await _bulkCopy.WriteToServerAsync(reader, cancellationToken).ConfigureAwait(false);
        }
    }
}
