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
            Options.ContextFactory = () => new Linq.InternalContext(new Database(connectionString, MsSqlProvider.Instance));
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 MySql 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        public void UseMySql(string connectionString)
        {
            Options.ContextFactory = () => new Linq.InternalContext(new Database(connectionString, MySqlProvider.Instance));
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 SQLite 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        public void UseSQLite(string connectionString)
        {
            Options.ContextFactory = () => new Linq.InternalContext(new Database(connectionString, SQLiteProvider.Instance));
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 Oracle 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        public void UseOracle(string connectionString)
        {
            Options.ContextFactory = () => new Linq.InternalContext(new Database(connectionString, OracleProvider.Instance));
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 Firebird 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        public void UseFirebird(string connectionString)
        {
            Options.ContextFactory = () => new Linq.InternalContext(new Database(connectionString, FirebirdProvider.Instance));
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 PostgreSql 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        public void UsePostgreSql(string connectionString)
        {
            Options.ContextFactory = () => new Linq.InternalContext(new Database(connectionString, PostgreSqlProvider.Instance));
        }
    }
}
