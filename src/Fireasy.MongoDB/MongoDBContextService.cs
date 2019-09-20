// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity;
using System;
using System.Data;

namespace Fireasy.MongoDB
{
    public class MongoDBContextService : ContextServiceBase
    {
        public MongoDBContextService(EntityContextInitializeContext context)
            : base (context)
        {
            Provider = context.Provider;
        }

        public override void BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            throw new NotImplementedException();
        }

        public override void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        public override void RollbackTransaction()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
        }
    }
}
