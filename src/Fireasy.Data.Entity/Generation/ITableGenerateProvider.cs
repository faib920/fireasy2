// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;
using System;

namespace Fireasy.Data.Entity.Generation
{
    public interface ITableGenerateProvider : IProviderService
    {
        bool IsExists(IDatabase database, Type entityType);

        void TryCreate(IDatabase database, Type entityType);
    }
}
