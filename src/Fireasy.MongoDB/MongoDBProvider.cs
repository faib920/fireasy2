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

namespace Fireasy.MongoDB
{
    public class MongoDBProvider : ProviderBase
    {
        public static MongoDBProvider Instance = new MongoDBProvider();

        public MongoDBProvider()
        {
            RegisterService<IContextProvider, MongoDBContextProvider>();
            RegisterService<IInjectionProvider, MongoDBInjectionProvider>();
        }

        public override string ProviderName => "MongoDB";

        /// <summary>
        /// 获取当前连接的参数。
        /// </summary>
        /// <returns></returns>
        public override ConnectionParameter GetConnectionParameter(ConnectionString connectionString)
        {
            return new ConnectionParameter
            {
                Server = connectionString.Properties["server"],
                Database = connectionString.Properties.TryGetValue("database")
            };
        }

        /// <summary>
        /// 使用参数更新指定的连接。
        /// </summary>
        /// <param name="connectionString">连接字符串对象。</param>
        /// <param name="parameter"></param>
        public override void UpdateConnectionString(ConnectionString connectionString, ConnectionParameter parameter)
        {
            connectionString.Properties.TrySetValue(parameter.Server, "server")
                .TrySetValue(parameter.Database, "database")
                .Update();
        }
    }
}
