﻿// -----------------------------------------------------------------------
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
using System.Data;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// SQLite数据库提供者。使用 System.Data.SQLite 提供。
    /// </summary>
    public class SQLiteProvider : ProviderBase
    {
        /// <summary>
        /// 提供 <see cref="SQLiteProvider"/> 的静态实例。
        /// </summary>
        public readonly static SQLiteProvider Instance = new SQLiteProvider();

        /// <summary>
        /// 初始化 <see cref="SQLiteProvider"/> 类的新实例。
        /// </summary>
        public SQLiteProvider()
#if NETSTANDARD
            : base(new AssemblyProviderFactoryResolver("System.Data.SQLite.SQLiteFactory, System.Data.SQLite", "Microsoft.Data.Sqlite.SqliteFactory, Microsoft.Data.Sqlite", "Microsoft.Data.Sqlite.SqliteFactory, Spreads.SQLite"))
#else
            : base(new AssemblyProviderFactoryResolver("System.Data.SQLite.SQLiteFactory, System.Data.SQLite"))
#endif
        {
        }

        public override string ProviderName => "SQLite";

        /// <summary>
        /// 获取当前连接的参数。
        /// </summary>
        /// <returns></returns>
        public override ConnectionParameter GetConnectionParameter(ConnectionString connectionString)
        {
            return new ConnectionParameter
            {
                Database = connectionString.Properties["data source"],
            };
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
                .Update();
        }

        /// <summary>
        /// 调整事务级别。
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public override IsolationLevel AmendIsolationLevel(IsolationLevel level)
        {
            return level == IsolationLevel.ReadUncommitted ?
                IsolationLevel.ReadCommitted : base.AmendIsolationLevel(level);
        }

        protected override void InitializeServices()
        {
            RegisterService<IGeneratorProvider, BaseSequenceGenerator>();
            RegisterService<ISyntaxProvider, SQLiteSyntax>();
            RegisterService<ISchemaProvider, SQLiteSchema>();
            RegisterService<IBatcherProvider, SQLiteBatcher>();
            RegisterService<IRecordWrapper, GeneralRecordWrapper>();
        }
    }
}
