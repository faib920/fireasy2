// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Data.Batcher;
using Fireasy.Data.Identity;
using Fireasy.Data.RecordWrapper;
using Fireasy.Data.Schema;
using Fireasy.Data.Syntax;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// MySql或MairaDB数据库提供者。使用 MySql.Data 提供。
    /// </summary>
    public class MySqlProvider : AssemblyProvider
    {
        /// <summary>
        /// 提供 <see cref="MySqlProvider"/> 的静态实例。
        /// </summary>
        public readonly static MySqlProvider Instance = new MySqlProvider();

        /// <summary>
        /// 初始化 <see cref="MySqlProvider"/> 类的新实例。
        /// </summary>
        public MySqlProvider()
#if NETSTANDARD2_0
            : base("MySql.Data.MySqlClient.MySqlClientFactory, MySqlConnector", 
                  "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data",
                  "Devart.Data.MySql.MySqlProviderFactory, Devart.Data.MySql")
#else
            : base("MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data")
#endif
        {
            RegisterService<IGeneratorProvider, BaseSequenceGenerator>();
            RegisterService<ISyntaxProvider, MySqlSyntax>();
            RegisterService<ISchemaProvider, MySqlSchema>();
            RegisterService<IBatcherProvider, MySqlBatcher>();
            RegisterService<IRecordWrapper, GeneralRecordWrapper>();
        }

        /// <summary>
        /// 获取描述数据库的名称。
        /// </summary>
        public override string DbName
        {
            get { return "mysql"; }
        }

        /// <summary>
        /// 获取当前连接的参数。
        /// </summary>
        /// <returns></returns>
        public override ConnectionParameter GetConnectionParameter(ConnectionString connectionString)
        {
            return new ConnectionParameter
            {
                Server = connectionString.Properties["data source"],
                Database = connectionString.Properties["database"],
                UserId = connectionString.Properties["user id"],
                Password = connectionString.Properties["password"],
            };
        }

        /// <summary>
        /// 使用参数更新指定的连接。
        /// </summary>
        /// <param name="connectionString">连接字符串对象。</param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override string UpdateConnectionString(ConnectionString connectionString, ConnectionParameter parameter)
        {
            connectionString.Properties.TrySetValue(parameter.Server, "data source")
                .TrySetValue(parameter.Database, "database")
                .TrySetValue(parameter.UserId, "user id")
                .TrySetValue(parameter.Password, "password");

            return connectionString.Update();
        }
    }
}
