// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;

namespace Fireasy.Data.Entity
{
    public class EntityContextOptionsBuilder
    {
        private EntityContext entityContext;

        public EntityContextOptionsBuilder(EntityContextOptions options)
        {
            Options = options;
        }

        public EntityContextOptions Options { get; private set; }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 SqlServer 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        public void UseSqlServer(string connectionString)
        {
            Options.ContextFactory = () => new EntityContextInitializeContext(MsSqlProvider.Instance, connectionString);
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 MySql 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        public void UseMySql(string connectionString)
        {
            Options.ContextFactory = () => new EntityContextInitializeContext(MySqlProvider.Instance, connectionString);
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 SQLite 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        public void UseSQLite(string connectionString)
        {
            Options.ContextFactory = () => new EntityContextInitializeContext(SQLiteProvider.Instance, connectionString);
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 Oracle 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        public void UseOracle(string connectionString)
        {
            Options.ContextFactory = () => new EntityContextInitializeContext(OracleProvider.Instance, connectionString);
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 Firebird 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        public void UseFirebird(string connectionString)
        {
            Options.ContextFactory = () => new EntityContextInitializeContext(FirebirdProvider.Instance, connectionString);
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 PostgreSql 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        public void UsePostgreSql(string connectionString)
        {
            Options.ContextFactory = () => new EntityContextInitializeContext(PostgreSqlProvider.Instance, connectionString);
        }
    }
}
