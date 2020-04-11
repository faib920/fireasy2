// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using Fireasy.Data.Configuration;
using Fireasy.Data.Entity.Query;
using Fireasy.Data.Provider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 提供以对象形式查询和使用实体数据的功能。
    /// </summary>
    public abstract class EntityContext : DisposeableBase, IServiceProvider, IServiceProviderAccessor, IObjectPoolable
    {
        private EntityContextOptions options;
        private IContextService service;
        private IObjectPool pool;

        /// <summary>
        /// 初始化 <see cref="EntityContext"/> 类的新实例。
        /// </summary>
        protected EntityContext()
            : this((string)null)
        {
        }

        /// <summary>
        /// 初始化 <see cref="EntityContext"/> 类的新实例。
        /// </summary>
        protected EntityContext(EntityContextOptions options)
            : this(null, options)
        {
        }

        /// <summary>
        /// 初始化 <see cref="EntityContext"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        protected EntityContext(IServiceProvider serviceProvider)
            : this(serviceProvider, new EntityContextOptions())
        {
        }

        /// <summary>
        /// 使用一个配置名称来初始化 <see cref="EntityContext"/> 类的新实例。
        /// </summary>
        /// <param name="instanceName">实例名称。</param>
        protected EntityContext(string instanceName)
            : this(null, new EntityContextOptions(instanceName))
        {
        }

        /// <summary>
        /// 初始化 <see cref="EntityContext"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="options">选项参数。</param>
        protected EntityContext(IServiceProvider serviceProvider, EntityContextOptions options)
        {
            this.options = options;
            ServiceProvider = serviceProvider;

            Initialize(options);

            new EntityRepositoryDiscoveryService(this, options).InitializeSets();
        }

        /// <summary>
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 获取关联的 <see cref="IDatabase"/> 实例。
        /// </summary>
        public IDatabase Database
        {
            get
            {
                if (service is IDatabaseAware aware)
                {
                    return aware.Database;
                }

                throw new NotSupportedException(SR.GetString(SRKind.NotSupportDatabase));
            }
        }

        /// <summary>
        /// 销毁资源。
        /// </summary>
        /// <param name="disposing">如果为 true，则同时释放托管资源和非托管资源；如果为 false，则仅释放非托管资源。</param>
        protected override bool Dispose(bool disposing)
        {
            if (pool == null)
            {
                Tracer.Debug($"The {GetType().Name} is Disposing.");

                service?.Dispose();
                service = null;

                options = null;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 为指定的类型返回 <see cref="EntityRepository{TEntity}"/>
        /// </summary>
        /// <typeparam name="TEntity">实体类型。</typeparam>
        /// <returns></returns>
        public IRepository<TEntity> Set<TEntity>() where TEntity : IEntity
        {
            var rep = service.CreateRepositoryProvider(typeof(TEntity)).CreateRepository(options);
            if (rep != null)
            {
                return (IRepository<TEntity>)rep;
            }

            return null;
        }

        /// <summary>
        /// 为指定的类型返回 <see cref="IRepository"/>
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <returns></returns>
        public IRepository Set(Type entityType)
        {
            return service.CreateRepositoryProvider(entityType).CreateRepository(options);
        }

        /// <summary>
        /// 创建树实体仓储实例。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public ITreeRepository<TEntity> CreateTreeRepository<TEntity>() where TEntity : class, IEntity
        {
            var repository = Set<TEntity>();
            return new EntityTreeRepository<TEntity>(repository, service);
        }

        /// <summary>
        /// 指定要包括在查询结果中的关联对象。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="fnMember">要包含的属性的表达式。</param>
        /// <returns></returns>
        public EntityContext Include<TEntity>(Expression<Func<TEntity, object>> fnMember) where TEntity : IEntity
        {
            InvokeQueryPolicy(q => q.IncludeWith(fnMember));

            return this;
        }

        /// <summary>
        /// 根据断言指定要包括在查询结果中的关联对象。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="isTrue">要计算的条件表达式。如果条件为 true，则进行 Include。</param>
        /// <param name="fnMember">要包含的属性的表达式。</param>
        /// <returns></returns>
        public EntityContext AssertInclude<TEntity>(bool isTrue, Expression<Func<TEntity, object>> fnMember) where TEntity : IEntity
        {
            if (isTrue)
            {
                InvokeQueryPolicy(q => q.IncludeWith(fnMember));
            }

            return this;
        }

        /// <summary>
        /// 对关联子实体集指定筛选表达式。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="memberQuery"></param>
        /// <returns></returns>
        public EntityContext Associate<TEntity>(Expression<Func<TEntity, IEnumerable>> memberQuery) where TEntity : IEntity
        {
            InvokeQueryPolicy(q => q.AssociateWith(memberQuery));

            return this;
        }

        /// <summary>
        /// 对实体指定筛选表达式。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="fnApply"></param>
        /// <returns></returns>
        public EntityContext Apply<TEntity>(Expression<Func<IEnumerable<TEntity>, IEnumerable<TEntity>>> fnApply) where TEntity : IEntity
        {
            InvokeQueryPolicy(q => q.Apply(fnApply));

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="fnApply"></param>
        /// <returns></returns>
        public EntityContext Apply(Type entityType, LambdaExpression fnApply)
        {
            InvokeQueryPolicy(q => q.Apply(entityType, fnApply));

            return this;
        }

        /// <summary>
        /// 配置参数。
        /// </summary>
        /// <param name="configuration">配置参数的方法。</param>
        /// <returns></returns>
        public EntityContext ConfigOptions(Action<EntityContextOptions> configuration)
        {
            configuration?.Invoke(options);

            return this;
        }

        /// <summary>
        /// 开始事务。
        /// </summary>
        /// <param name="level"></param>
        public void BeginTransaction(IsolationLevel? level = null)
        {
            level ??= options.IsolationLevel;
            service.BeginTransaction(level.Value);
        }

        /// <summary>
        /// 提交事务。
        /// </summary>
        public void CommitTransaction()
        {
            service.CommitTransaction();
        }

        /// <summary>
        /// 回滚事务。
        /// </summary>
        public void RollbackTransaction()
        {
            service.RollbackTransaction();
        }

        /// <summary>
        /// 初始化。
        /// </summary>
        private void Initialize(EntityContextOptions options)
        {
            Guard.ArgumentNull(options, nameof(options));

            TrySetContextType(options);
            TrySetServiceProvider(options);
            TryPaserInstanceSetting(options);

            var builder = new EntityContextOptionsBuilder(options);
            OnConfiguring(builder);

            if (options.Provider == null)
            {
                throw new NotSupportedException(SR.GetString(SRKind.NotSupportDbProvider));
            }

            var contextProvider = options.GetProviderService<IContextProvider>();

            service = contextProvider.CreateContextService(new ContextServiceContext(options));
        }

        private void TrySetContextType(EntityContextOptions options)
        {
            if (options is IInstanceIdentifier identifier && identifier.ContextType == null)
            {
                identifier.ContextType = GetType();
            }
        }

        /// <summary>
        /// 尝试初始化 <see cref="IServiceProvider"/> 实例。
        /// </summary>
        /// <param name="options"></param>
        protected virtual void TrySetServiceProvider(EntityContextOptions options)
        {
            if (options is IInstanceIdentifier identification)
            {
                if (identification.ServiceProvider == null && ServiceProvider != null)
                {
                    identification.ServiceProvider = ServiceProvider;
                }
                else if (ServiceProvider == null && identification.ServiceProvider != null)
                {
                    ServiceProvider = identification.ServiceProvider;
                }
                else if (identification.ServiceProvider == null && ServiceProvider == null)
                {
                    ServiceProvider = identification.ServiceProvider = ContainerUnity.GetContainer();
                }
            }

            if (ServiceProvider == null)
            {
                ServiceProvider = ContainerUnity.GetContainer();
            }
        }

        /// <summary>
        /// 尝试从配置文件中配置 <see cref="EntityContextOptions"/> 实例。
        /// </summary>
        /// <param name="options"></param>
        protected virtual void TryPaserInstanceSetting(EntityContextOptions options)
        {
            if (options.Provider == null && options.ConnectionString == null)
            {
                var section = ConfigurationUnity.GetSection<InstanceConfigurationSection>();
                if (section != null)
                {
                    IInstanceConfigurationSetting setting;

                    if (!string.IsNullOrEmpty(options.ConfigName))
                    {
                        setting = section.Settings[options.ConfigName];
                    }
                    else
                    {
                        setting = section.Default;
                    }

                    if (setting != null)
                    {
                        options.Provider = ProviderHelper.GetDefinedProviderInstance(setting);
                        options.ConnectionString = setting.ConnectionString;
                    }
                }
            }
        }

        /// <summary>
        /// 使用构造器进行配置。
        /// </summary>
        /// <param name="builder">构造器。</param>
        protected virtual void OnConfiguring(EntityContextOptionsBuilder builder)
        {
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IContextService))
            {
                return service;
            }
            else if (serviceType == typeof(IServiceProvider))
            {
                return ServiceProvider;
            }
            else if (serviceType == typeof(IDatabase) && service is IDatabaseAware aware)
            {
                return aware.Database;
            }
            else if (serviceType == typeof(IProvider))
            {
                return service.Provider;
            }

            return null;
        }

        public T GetService<T>()
        {
            var svr = GetService(typeof(T));
            if (svr != null)
            {
                return (T)svr;
            }

            return default;
        }

        private void InvokeQueryPolicy(Action<IQueryPolicy> action)
        {
            if (service is IQueryPolicy policy)
            {
                action(policy);
            }
            else if (service is IQueryPolicyAware aware && aware.QueryPolicy != null)
            {
                action(aware.QueryPolicy);
            }
        }

        void IObjectPoolable.SetPool(IObjectPool pool)
        {
            this.pool = pool;
        }
    }
}
