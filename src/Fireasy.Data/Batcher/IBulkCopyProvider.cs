// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Batcher
{
    public interface IBulkCopyProvider : IProviderService, IDisposable
    {
        void Initialize(DbConnection connection, DbTransaction transaction, string tableName, int batchSize);

        void AddColumnMapping(int sourceColumnIndex, string destinationColumn);

        void WriteToServer(DbDataReader reader);

        Task WriteToServerAsync(DbDataReader reader, CancellationToken cancellationToken);
    }
}
