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
using System.Data;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// <see cref="EntityContext"/> 的参数。
    /// </summary>
    public class EntityContextOptions : IInstanceIdentifier
    {
        private IServiceProvider serviceProvider;

        /// <summary>
        /// 初始化 <see cref="EntityContextOptions"/> 类的新实例。
        /// </summary>
        public EntityContextOptions()
        {
            Initializers = new EntityContextPreInitializerCollection();
            Initializers.Add<RecompileAssemblyPreInitializer>();
        }

        /// <summary>
        /// 初始化 <see cref="EntityContextOptions"/> 类的新实例。
        /// </summary>
        /// <param name="configName">配置中的实例名称。</param>
        public EntityContextOptions(string configName)
            : this()
        {
            ConfigName = configName;
        }

        /// <summary>
        /// 初始化 <see cref="EntityContextOptions"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider">应用程序服务提供者实例。</param>
        internal EntityContextOptions(IServiceProvider serviceProvider)
            : this()
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 获取或设置是否通知持久化事件。默认为 false。
        /// </summary>
        public bool NotifyEvents { get; set; } = false;

        /// <summary>
        /// 获取或设置是否验证实体属性。默认为 true。
        /// </summary>
        public bool ValidateEntity { get; set; } = true;

        /// <summary>
        /// 获取或设置是否允许分配默认值。
        /// </summary>
        public bool AllowDefaultValue { get; set; } = true;

        /// <summary>
        /// 获取或设置默认的事务级别。默认为允许脏读。
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadUncommitted;

        /// <summary>
        /// 获取或设置配置中的实例名称。
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// 获取或设置是否开启解析缓存。
        /// </summary>
        public bool? CacheParsing { get; set; }

        /// <summary>
        /// 获取或设置解析缓存过期时间。
        /// </summary>
        public TimeSpan? CacheParsingTimes { get; set; }

        /// <summary>
        /// 获取或设置是否开启数据缓存。
        /// </summary>
        public bool? CacheExecution { get; set; }

        /// <summary>
        /// 获取或设置数据缓存过期时间。
        /// </summary>
        public TimeSpan? CacheExecutionTimes { get; set; }

        /// <summary>
        /// 获取或设置关联加载的行为。默认为 <see cref="Lazy"/>。
        /// </summary>
        public LoadBehavior LoadBehavior { get; set; } = LoadBehavior.Lazy;

        /// <summary>
        /// 获取初始化方法。
        /// </summary>
        public EntityContextPreInitializerCollection Initializers { get; private set; }

        /// <summary>
        /// 获取或设置数据库提供者。
        /// </summary>
        public IProvider Provider { get; set; }

        IServiceProvider IInstanceIdentifier.ServiceProvider
        {
            get { return serviceProvider; }
            set { serviceProvider = value; }
        }

        Type IInstanceIdentifier.ContextType { get; set; }

        /// <summary>
        /// 获取或设置数据库连接串。
        /// </summary>
        public ConnectionString ConnectionString { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is EntityContextOptions op))
            {
                return false;
            }

            return NotifyEvents == op.NotifyEvents &&
                ValidateEntity == op.ValidateEntity &&
                IsolationLevel == op.IsolationLevel &&
                AllowDefaultValue == op.AllowDefaultValue &&
                ConfigName == op.ConfigName &&
                CacheParsing == op.CacheParsing &&
                CacheParsingTimes == op.CacheParsingTimes &&
                CacheExecution == op.CacheExecution &&
                CacheExecutionTimes == op.CacheExecutionTimes &&
                LoadBehavior == op.LoadBehavior &&
                Provider == op.Provider &&
                ConnectionString == op.ConnectionString;
        }
    }

    /// <summary>
    /// 泛型的 <see cref="EntityContext"/> 的参数。
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class EntityContextOptions<TContext> : EntityContextOptions where TContext : EntityContext
    {
        /// <summary>
        /// 初始化 <see cref="EntityContextOptions{TContext}"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider">应用程序服务提供者实例。</param>
        internal EntityContextOptions(IServiceProvider serviceProvider)
            : base (serviceProvider)
        {
            (this as IInstanceIdentifier).ContextType = typeof(TContext);
        }
    }
}
