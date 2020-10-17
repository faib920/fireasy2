// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity;
using Fireasy.Data.Provider;

namespace Fireasy.MongoDB
{
    public sealed class MongoDBContextProvider : IContextProvider
    {
        IProvider IProviderService.Provider { get; set; }

        IContextService IContextProvider.CreateContextService(ContextServiceContext context)
        {
            return new MongoDBContextService(context);
        }
    }
}
