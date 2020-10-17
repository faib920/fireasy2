// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using Fireasy.Common.Reflection;
using Fireasy.Data.Batcher;
using Fireasy.Data.Identity;
using Fireasy.Data.RecordWrapper;
using Fireasy.Data.Schema;
using Fireasy.Data.Syntax;
using System.Data.Common;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// Oracle数据库提供者。
    /// </summary>
    public class OracleProvider : ProviderBase
    {
        /// <summary>
        /// 提供 <see cref="OracleProvider"/> 的静态实例。
        /// </summary>
        public readonly static OracleProvider Instance = new OracleProvider();

        /// <summary>
        /// 初始化 <see cref="OracleProvider"/> 类的新实例。
        /// </summary>
        public OracleProvider()
#if NETSTANDARD
            : base(new AssemblyProviderFactoryResolver("Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess", "System.Data.OracleClient.OracleClientFactory, Mono.Data.OracleClientCore"))
#else
            : base(new AssemblyProviderFactoryResolver("Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess", "Oracle.DataAccess.Client.OracleClientFactory, Oracle.DataAccess"),
                  new InstallerProviderFactoryResolver("System.Data.OracleClient"))
#endif
        {
        }

        public override string ProviderName => "Oracle";

        /// <summary>
        /// 获取当前连接的参数。
        /// </summary>
        /// <returns></returns>
        public override ConnectionParameter GetConnectionParameter(ConnectionString connectionString)
        {
            return new ConnectionParameter
            {
                Server = connectionString.Properties["data source"],
                UserId = connectionString.Properties["user id"],
                Password = connectionString.Properties["password"]
            };
        }

        /// <summary>
        /// 使用参数更新指定的连接。
        /// </summary>
        /// <param name="connectionString">连接字符串对象。</param>
        /// <param name="parameter"></param>
        public override void UpdateConnectionString(ConnectionString connectionString, ConnectionParameter parameter)
        {
            connectionString.Properties.TrySetValue(parameter.Server, "data source")
                .TrySetValue(parameter.UserId, "user id")
                .TrySetValue(parameter.Password, "password")
                .Update();
        }

        /// <summary>
        /// 处理 <see cref="DbCommand"/> 对象。
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override DbCommand PrepareCommand(DbCommand command)
        {
            //处理 ORA-01008: 并非所有变量都已绑定 ，将 BindByName 设为 true
            var property = ReflectionCache.GetMember("OracleCommand_BindByName", command.GetType(), k => k.GetProperty("BindByName"));
            if (property != null)
            {
                property.FastSetValue(command, true);
            }

            return command;
        }

        protected override void InitializeServices()
        {
            RegisterService<IGeneratorProvider, OracleSequenceGenerator>();
            RegisterService<ISyntaxProvider, OracleSyntax>();
            RegisterService<ISchemaProvider, OracleSchema>();
            RegisterService<IBatcherProvider, OracleDABatcher>();
            RegisterService<IRecordWrapper, OracleRecordWrapper>();
        }
    }
}
