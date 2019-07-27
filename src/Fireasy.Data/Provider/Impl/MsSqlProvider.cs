// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Data.Backup;
using Fireasy.Data.Batcher;
using Fireasy.Data.Identity;
using Fireasy.Data.RecordWrapper;
using Fireasy.Data.Schema;
using Fireasy.Data.Syntax;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// MsSql数据库提供者。
    /// </summary>
    public class MsSqlProvider : ProviderBase
    {
        /// <summary>
        /// 提供 <see cref="MsSqlProvider"/> 的静态实例。
        /// </summary>
        public readonly static MsSqlProvider Instance = new MsSqlProvider();

        /// <summary>
        /// 初始化 <see cref="MsSqlProvider"/> 类的新实例。
        /// </summary>
        public MsSqlProvider()
#if NETSTANDARD
            : base(new AssemblyProviderFactoryResolver("System.Data.SqlClient.SqlClientFactory, System.Data.SqlClient"))
#else
            : base(new InstallerProviderFactoryResolver("System.Data.SqlClient"))
#endif
        {
            RegisterService<IGeneratorProvider, BaseSequenceGenerator>();
            RegisterService<IBackupProvider, MsSqlBackup>();
            RegisterService<ISyntaxProvider, MsSqlSyntax>();
            RegisterService<ISchemaProvider, MsSqlSchema>();
            RegisterService<IBatcherProvider, MsSqlBatcher>();
            RegisterService<IRecordWrapper, GeneralRecordWrapper>();
        }

        public override string ProviderName => "SqlServer";

        /// <summary>
        /// 获取当前连接的参数。
        /// </summary>
        /// <returns></returns>
        public override ConnectionParameter GetConnectionParameter(ConnectionString connectionString)
        {
            return new ConnectionParameter
            {
                Server = connectionString.Properties.TryGetValue("data source", "server"),
                Database = connectionString.Properties.TryGetValueWithDefaultValue("master", "initial catalog", "database"),
                UserId = connectionString.Properties.TryGetValue("user id", "uid"),
                Password = connectionString.Properties.TryGetValue("password", "pwd"),
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
            connectionString.Properties.TrySetValue(parameter.Server, "data source", "server")
                .TrySetValue(parameter.Database, "initial catalog", "database")
                .TrySetValue(parameter.UserId, "user id", "uid")
                .TrySetValue(parameter.Password, "password", "pwd");

            return connectionString.Update();
        }
    }
}
