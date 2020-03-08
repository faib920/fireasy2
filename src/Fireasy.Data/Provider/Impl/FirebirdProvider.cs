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
    /// Firebird数据库提供者。使用 FirebirdSql.Data.FirebirdClient 提供。
    /// </summary>
    public class FirebirdProvider : ProviderBase
    {
        /// <summary>
        /// 提供 <see cref="FirebirdProvider"/> 的静态实例。
        /// </summary>
        public readonly static FirebirdProvider Instance = new FirebirdProvider();

        /// <summary>
        /// 初始化 <see cref="PostgreSqlProvider"/> 类的新实例。
        /// </summary>
        public FirebirdProvider()
            : base(new AssemblyProviderFactoryResolver("FirebirdSql.Data.FirebirdClient.FirebirdClientFactory, FirebirdSql.Data.FirebirdClient"))
        {
            RegisterService<IGeneratorProvider, BaseSequenceGenerator>();
            RegisterService<ISyntaxProvider, FirebirdSyntax>();
            RegisterService<ISchemaProvider, FirebirdSchema>();
            RegisterService<IBatcherProvider, MySqlBatcher>();
            RegisterService<IRecordWrapper, GeneralRecordWrapper>();
        }

        public override string ProviderName => "Firebird";

        /// <summary>
        /// 获取当前连接的参数。
        /// </summary>
        /// <returns></returns>
        public override ConnectionParameter GetConnectionParameter(ConnectionString connectionString)
        {
            return new ConnectionParameter
            {
                Server = connectionString.Properties["server"],
                Database = connectionString.Properties.TryGetValue("database", "initial catalog"),
                UserId = connectionString.Properties.TryGetValue("userid", "user id"),
                Password = connectionString.Properties.TryGetValue("password", "pwd"),
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
                .TrySetValue(parameter.Database, "database", "initial catalog")
                .TrySetValue(parameter.UserId, "userid", "user id")
                .TrySetValue(parameter.Password, "password", "pwd")
                .Update();
        }
    }
}
