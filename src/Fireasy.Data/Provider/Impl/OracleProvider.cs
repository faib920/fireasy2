// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
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
    public class OracleProvider : AssemblyProvider
    {
        /// <summary>
        /// 提供 <see cref="OracleProvider"/> 的静态实例。
        /// </summary>
        public readonly static OracleProvider Instance = new OracleProvider();

        /// <summary>
        /// 初始化 <see cref="OracleProvider"/> 类的新实例。
        /// </summary>
        public OracleProvider()
#if NETSTANDARD2_0
            : base("System.Data.OracleClient.OracleClientFactory, Mono.Data.OracleClientCore", 
                  "Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess",
                  "Devart.Data.Oracle.OracleClientFactory, Devart.Data.Oracle")
#else
            : base("System.Data.OracleClient.OracleClientFactory, System.Data.OracleClient",
                  "Oracle.DataAccess.Client.OracleClientFactory, Oracle.DataAccess",
                  "Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess")
#endif
        {
            RegisterService<IGeneratorProvider, OracleSequenceGenerator>();
            RegisterService<ISyntaxProvider, OracleSyntax>();
            RegisterService<ISchemaProvider, OracleSchema>();
            RegisterService<IRecordWrapper, OracleRecordWrapper>();
        }

        /// <summary>
        /// 获取描述数据库的名称。
        /// </summary>
        public override string DbName
        {
            get { return "oracle"; }
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
                Schema = connectionString.Properties["user id"].ToUpper(),
                UserId = connectionString.Properties["user id"],
                Password = connectionString.Properties["password"]
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
                .TrySetValue(parameter.UserId, "user id")
                .TrySetValue(parameter.Password, "password");

            return connectionString.Update();
        }

        /// <summary>
        /// 处理 <see cref="DbCommand"/> 对象。
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override DbCommand PrepareCommand(DbCommand command)
        {
            //处理 ORA-01008: 并非所有变量都已绑定 ，将 BindByName 设为 true
            var property = command.GetType().GetProperty("BindByName");
            if (property != null)
            {
                property.FastSetValue(command, true);
            }

            return command;
        }
    }
}
