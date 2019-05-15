// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data;
using Fireasy.Data.Entity;
using Fireasy.Data.Provider;
using System;

namespace Fireasy.MongoDB
{
    public class MongoDBProvider : ProviderBase
    {
        public static MongoDBProvider Instance = new MongoDBProvider();

        public MongoDBProvider()
        {
            RegisterService<IContextProvider, MongoDBContextProvider>();
        }

        public override string ProviderName => "MongoDB";

        public override ConnectionParameter GetConnectionParameter(ConnectionString connectionString)
        {
            throw new NotImplementedException();
        }

        public override string UpdateConnectionString(ConnectionString connectionString, ConnectionParameter parameter)
        {
            throw new NotImplementedException();
        }
    }
}
