// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Identity;
using Fireasy.Data.RecordWrapper;
using Fireasy.Data.Schema;
using Fireasy.Data.Syntax;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// OleDb数据库提供者。
    /// </summary>
    public class OleDbProvider : ProviderBase
    {
        /// <summary>
        /// 提供 <see cref="OleDbProvider"/> 的静态实例。
        /// </summary>
        public readonly static OleDbProvider Instance = new OleDbProvider();

        /// <summary>
        /// 初始化 <see cref="OleDbProvider"/> 类的新实例。
        /// </summary>
        public OleDbProvider()
#if NETFRAMEWORK
            : base(new InstallerProviderFactoryResolver("System.Data.OleDb"))
#else
            : base(new AssemblyProviderFactoryResolver("System.Data.OleDb.OleDbFactory, System.Data.OleDb"))
#endif
        {
#if NETFRAMEWORK
            RegisterService<ISchemaProvider, OleDbSchema>();
#endif
            RegisterService<IGeneratorProvider, BaseSequenceGenerator>();
            RegisterService<IRecordWrapper, GeneralRecordWrapper>();
        }

        public override string ProviderName => "OleDb";

        /// <summary>
        /// 获取当前连接的参数。
        /// </summary>
        /// <returns></returns>
        public override ConnectionParameter GetConnectionParameter(ConnectionString connectionString)
        {
            var provider = connectionString.Properties["provider"];

            if (provider.IndexOf("SQLOLEDB") != -1)
            {
                return new ConnectionParameter
                {
                    Database = connectionString.Properties["initial catalog"],
                    UserId = connectionString.Properties["user id"],
                    Password = connectionString.Properties["password"]
                };
            }

            return null;
        }

        /// <summary>
        /// 使用参数更新指定的连接。
        /// </summary>
        /// <param name="connectionString">连接字符串对象。</param>
        /// <param name="parameter"></param>
        public override void UpdateConnectionString(ConnectionString connectionString, ConnectionParameter parameter)
        {
            connectionString.Properties
                .TrySetValue(parameter.Database, "data source")
                .TrySetValue(parameter.UserId, "user id")
                .TrySetValue(parameter.Password, "password")
                .Update();
        }
    }
}