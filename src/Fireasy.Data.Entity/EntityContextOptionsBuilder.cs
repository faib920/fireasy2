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
    /// <summary>
    /// <see cref="EntityContextOptions"/> 构造器。
    /// </summary>
    public class EntityContextOptionsBuilder
    {
        private readonly Type contextType;

        /// <summary>
        /// 初始化 <see cref="EntityContextOptionsBuilder"/> 类的新实例。
        /// </summary>
        /// <param name="options"></param>
        public EntityContextOptionsBuilder(EntityContextOptions options)
        {
            Options = options;

            if (Options is IInstanceIdentifier identifier && identifier.ContextType != null)
            {
                contextType = identifier.ContextType;
            }
        }

        /// <summary>
        /// 初始化 <see cref="EntityContextOptionsBuilder"/> 类的新实例。
        /// </summary>
        /// <param name="contextType"></param>
        /// <param name="options"></param>
        public EntityContextOptionsBuilder(Type contextType, EntityContextOptions options)
            : this(options)
        {
            this.contextType = contextType;
        }

        /// <summary>
        /// 获取 <see cref="EntityContextOptions"/> 参数。
        /// </summary>
        public EntityContextOptions Options { get; private set; }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 SqlServer 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseSqlServer(string connectionString)
        {
            Options.Provider = MsSqlProvider.Instance;
            Options.ConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 MySql 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseMySql(string connectionString)
        {
            Options.Provider = MySqlProvider.Instance;
            Options.ConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 SQLite 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseSQLite(string connectionString)
        {
            Options.Provider = SQLiteProvider.Instance;
            Options.ConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 Oracle 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseOracle(string connectionString)
        {
            Options.Provider = OracleProvider.Instance;
            Options.ConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 Firebird 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseFirebird(string connectionString)
        {
            Options.Provider = FirebirdProvider.Instance;
            Options.ConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 PostgreSql 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UsePostgreSql(string connectionString)
        {
            Options.Provider = PostgreSqlProvider.Instance;
            Options.ConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// 使用 CodeFirst 模式。
        /// </summary>
        /// <param name="changedAction"></param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseCodeFirst(Action<RespositoryChangedEventArgs> changedAction = null)
        {
            Options.Initializers.Add<RespositoryCreatePreInitializer>(s => s.EventHandler = changedAction);
            return this;
        }

        /// <summary>
        /// 使用持久化环境进行分表配置。
        /// </summary>
        /// <param name="setupAction">对 <see cref="EntityPersistentEnvironment"/> 进行配置的方法。</param>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseEnvironment(Action<EntityPersistentEnvironment> setupAction)
        {
            var environment = new EntityPersistentEnvironment();
            setupAction?.Invoke(environment);
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
        /// <typeparam name="TEntity">指定具体的实体类。</typeparam>
        /// <returns></returns>
        public EntityContextOptionsBuilder UseOracleTrigger<TEntity>() where TEntity : IEntity
        {
            Options.Initializers.Add<OracleTriggerPreInitializer>(s => s.Add(typeof(TEntity)));
            return this;
        }
    }
}
