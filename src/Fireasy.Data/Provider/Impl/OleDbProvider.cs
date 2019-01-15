// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETSTANDARD
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
            : base("System.Data.OleDb")
        {
            RegisterService<IGeneratorProvider, BaseSequenceGenerator>();
            RegisterService<IRecordWrapper, GeneralRecordWrapper>();
        }

        /// <summary>
        /// 获取描述数据库的名称。
        /// </summary>
        public override string DbName
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// 获取当前连接的参数。
        /// </summary>
        /// <returns></returns>
        public override ConnectionParameter GetConnectionParameter(ConnectionString connectionString)
        {
            var provider = connectionString.Properties["provider"];

            var parameter = new ConnectionParameter
            {
                Database = connectionString.Properties["data source"],
                UserId = connectionString.Properties["user id"],
                Password = connectionString.Properties["password"]
            };

            switch (provider.ToUpper())
            {
                case "SQLOLEDB":
                    parameter.Schema = "dbo";
                    break;
                case "MSDAORA":
                case "MSDAORA.1":
                    parameter.Schema = connectionString.Properties["user id"].ToUpper();
                    break;
            }

            return parameter;
        }

        /// <summary>
        /// 使用参数更新指定的连接。
        /// </summary>
        /// <param name="connectionString">连接字符串对象。</param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override string UpdateConnectionString(ConnectionString connectionString, ConnectionParameter parameter)
        {
            connectionString.Properties
                .TrySetValue(parameter.Database, "data source")
                .TrySetValue(parameter.UserId, "user id")
                .TrySetValue(parameter.Password, "password");

            return connectionString.Update();
        }
    }
}
#endif