// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Data.Common;

namespace Fireasy.Data.MsSqlClient
{
    public class MsSqlProvider : Provider.MsSqlProvider
    {
        public override DbProviderFactory DbProviderFactory => Microsoft.Data.SqlClient.SqlClientFactory.Instance;

        protected override void InitializeServices()
        {
            base.InitializeServices();
            RegisterService<Batcher.IBulkCopyProvider, SqlBulkCopyProvider>();
        }
    }
}
