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
using System;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// OleDb数据库提供者。
    /// </summary>
    public class OleDbProvider : ProviderBase
    {
        private class OleDbFeature
        {
            public const string Excel = "Excel";
        }

        /// <summary>
        /// 提供 <see cref="OleDbProvider"/> 的静态实例。
        /// </summary>
        public readonly static OleDbProvider Instance = new OleDbProvider();

        /// <summary>
        /// 初始化 <see cref="OleDbProvider"/> 类的新实例。
        /// </summary>
        public OleDbProvider()
#if NETSTANDARD2_0
            : base(new AssemblyProviderFactoryResolver("System.Data.OleDb.OleDbFactory, System.Data.OleDb"))
#else
            : base(new InstallerProviderFactoryResolver("System.Data.OleDb"))
#endif
        {
        }

        public override string ProviderName => "OleDb";

        public override bool HasFeature => true;

        /// <summary>
        /// 获取当前连接的参数。
        /// </summary>
        /// <returns></returns>
        public override ConnectionParameter GetConnectionParameter(ConnectionString connectionString)
        {
            var provider = connectionString.Properties["provider"];

            if (IsSqlServer(provider))
            {
                return new ConnectionParameter
                {
                    Database = connectionString.Properties["initial catalog"],
                    UserId = connectionString.Properties["user id"],
                    Password = connectionString.Properties["password"]
                };
            }
            else if (IsMsDac(provider))
            {
                return new ConnectionParameter
                {
                    Database = connectionString.Properties["data source"],
                    Password = connectionString.Properties["Jet OLEDB:database password"]
                };
            }
            else if (IsOracle(provider))
            {
                return new ConnectionParameter
                {
                    Database = connectionString.Properties["data source"],
                    UserId = connectionString.Properties["user id"],
                    Password = connectionString.Properties["password"]
                };
            }
            else
            {
                return new ConnectionParameter
                {
                    Database = connectionString.Properties["data source"]
                };
            }
        }

        /// <summary>
        /// 使用参数更新指定的连接。
        /// </summary>
        /// <param name="connectionString">连接字符串对象。</param>
        /// <param name="parameter"></param>
        public override void UpdateConnectionString(ConnectionString connectionString, ConnectionParameter parameter)
        {
            var provider = connectionString.Properties["provider"];

            if (IsSqlServer(provider))
            {
                connectionString.Properties
                    .TrySetValue(parameter.Database, "initial catalog")
                    .TrySetValue(parameter.UserId, "user id")
                    .TrySetValue(parameter.Password, "password")
                    .Update();
            }
            else if (IsMsDac(provider))
            {
                connectionString.Properties
                    .TrySetValue(parameter.Database, "data source")
                    .TrySetValue(parameter.Password, "Jet OLEDB:database password")
                    .Update();
            }
            else if (IsOracle(provider))
            {
                connectionString.Properties
                    .TrySetValue(parameter.Database, "data source")
                    .TrySetValue(parameter.UserId, "user id")
                    .TrySetValue(parameter.Password, "password")
                    .Update();
            }
            else
            {
                connectionString.Properties
                    .TrySetValue(parameter.Database, "data source")
                    .Update();
            }
        }

        protected virtual bool IsSqlServer(string provider)
        {
            return provider.IndexOf("sqloledb", StringComparison.OrdinalIgnoreCase) != -1;
        }

        protected virtual bool IsMsDac(string provider)
        {
            return provider.IndexOf("microsoft.jet.oledb.", StringComparison.OrdinalIgnoreCase) != -1 ||
                provider.IndexOf("microsoft.ace.oledb.", StringComparison.OrdinalIgnoreCase) != -1;
        }

        protected virtual bool IsOracle(string provider)
        {
            return provider.IndexOf("msdaora", StringComparison.OrdinalIgnoreCase) != -1;
        }

        protected virtual bool IsFoxpro(string provider)
        {
            return provider.IndexOf("vfpoledb", StringComparison.OrdinalIgnoreCase) != -1;
        }

        public override string GetFeature(ConnectionString connectionString)
        {
            var provider = connectionString.Properties["provider"];

            if (IsMsDac(provider))
            {
                var extended = connectionString.Properties.TryGetValue("extended properties");
                if (extended.IndexOf("excel", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    return OleDbFeature.Excel;
                }
            }

            return string.Empty;
        }

        public override IProvider Clone(string feature)
        {
            return feature switch
            {
                OleDbFeature.Excel => base.Clone(feature)
                    .RegisterService(typeof(OleDbSyntax4Excel)),
                _ => this,
            };
        }

        protected override void InitializeServices()
        {
            RegisterService<ISchemaProvider, OleDbSchema>();
            RegisterService<ISyntaxProvider, OleDbSyntax>();
            RegisterService<IGeneratorProvider, BaseSequenceGenerator>();
            RegisterService<IRecordWrapper, GeneralRecordWrapper>();
        }
    }
}