// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Initializers;
using Fireasy.Data.Provider;
using System;

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
        /// <returns></returns>
        public EntityContextOptionsBuilder UseSqlServer(string connectionString)
        {
            Options.ContextFactory = () => new EntityContextInitializeContext(MsSqlProvider.Instance, connectionString);
            return this;
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 MySql 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseMySql(string connectionString)
        {
            Options.ContextFactory = () => new EntityContextInitializeContext(MySqlProvider.Instance, connectionString);
            return this;
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 SQLite 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseSQLite(string connectionString)
        {
            Options.ContextFactory = () => new EntityContextInitializeContext(SQLiteProvider.Instance, connectionString);
            return this;
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 Oracle 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseOracle(string connectionString)
        {
            Options.ContextFactory = () => new EntityContextInitializeContext(OracleProvider.Instance, connectionString);
            return this;
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 Firebird 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseFirebird(string connectionString)
        {
            Options.ContextFactory = () => new EntityContextInitializeContext(FirebirdProvider.Instance, connectionString);
            return this;
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 PostgreSql 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UsePostgreSql(string connectionString)
        {
            Options.ContextFactory = () => new EntityContextInitializeContext(PostgreSqlProvider.Instance, connectionString);
            return this;
        }

        /// <summary>
        /// 使用 CodeFirst 模式。
        /// </summary>
        /// <param name="changed"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseCodeFirst(Action<RespositoryChangedEventArgs> changed = null)
        {
            Options.Initializers.Add<RespositoryCreatePreInitializer>(s => s.EventHandler = changed);
            return this;
        }

        /// <summary>
        /// 使用持久化环境进行分表配置。
        /// </summary>
        /// <param name="setup">对 <see cref="EntityPersistentEnvironment"/> 进行配置的方法。</param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseEnvironment(Action<EntityPersistentEnvironment> setup)
        {
            var environment = new EntityPersistentEnvironment();
            setup?.Invoke(environment);
            Options.Initializers.Add<EnvironmentPreInitializer>(s => s.Environment = environment);
            return this;
        }

        /// <summary>
        /// 针对 Oracle 数据库，采用触发器将序列值作为新增数据的主键值。
        /// </summary>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseOracleTrigger()
        {
            Options.Initializers.Add<OracleTriggerPreInitializer>();
            return this;
        }

        /// <summary>
        /// 针对 Oracle 数据库，采用触发器将序列值作为新增数据的主键值。
        /// </summary>
        /// <typeparam name="T">指定具体的实体类。</typeparam>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseOracleTrigger<T>() where T : IEntity
        {
            Options.Initializers.Add<OracleTriggerPreInitializer>(s => s.Add(typeof(T)));
            return this;
        }
    }
}
