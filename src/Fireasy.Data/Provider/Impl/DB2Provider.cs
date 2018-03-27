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
    /// DB2数据库提供者。使用 IBM.Data.DB2 提供。
    /// </summary>
    public class DB2Provider : AssemblyProvider
    {
        /// <summary>
        /// 提供 <see cref="DB2Provider"/> 的静态实例。
        /// </summary>
        public readonly static DB2Provider Instance = new DB2Provider();

        /// <summary>
        /// 初始化 <see cref="DB2Provider"/> 类的新实例。
        /// </summary>
        public DB2Provider()
            : base("IBM.Data.DB2.DB2Factory, IBM.Data.DB2")
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
            get { return "db2"; }
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
