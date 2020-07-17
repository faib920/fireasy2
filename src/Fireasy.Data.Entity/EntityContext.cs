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
using Fireasy.Common.MultiTenancy;
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
        private EntityContextOptions _options;
        private IContextService _contextService;
        private string _poolName;
        private IServiceProvider _serviceProvider;

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
            _options = options;
            _serviceProvider = serviceProvider;

            Initialize(options);

            new EntityRepositoryDiscoveryService(this, options).InitializeSets();
        }

        /// <summary>
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
            set
            {
                _serviceProvider = value;
                _contextService.ServiceProvider = value;
            }
        }

        /// <summary>
        /// 获取关联的 <see cref="IDatabase"/> 实例。
        /// </summary>
        public IDatabase Database
        {
            get
            {
                if (_contextService is IDatabaseAware aware)
                {
                    return aware.Database;
                }

                throw new NotSupportedException(SR.GetString(SRKind.NotSupportDatabase));
            }
        }

        string IObjectPoolable.PoolName
        {
            get { return _poolName; }
        }

        /// <summary>
        /// 销毁资源。
        /// </summary>
        /// <param name="disposing">如果为 true，则同时释放托管资源和非托管资源；如果为 false，则仅释放非托管资源。</param>
        protected override bool Dispose(bool disposing)
        {
            if (_poolName == null)
            {
                Tracer.Debug($"The {GetType().Name} is Disposing.");

                _contextService?.Dispose();
                _contextService = null;

                _options = null;

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
            var rep = _contextService.CreateRepositoryProvider(typeof(TEntity)).CreateRepository(_options);
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
            return _contextService.CreateRepositoryProvider(entityType).CreateRepository(_options);
        }

        /// <summary>
        /// 创建树实体仓储实例。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public ITreeRepository<TEntity> CreateTreeRepository<TEntity>() where TEntity : class, IEntity
        {
            var repository = Set<TEntity>();
            return new EntityTreeRepository<TEntity>(repository, _contextService);
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
            configuration?.Invoke(_options);

            return this;
        }

        /// <summary>
        /// 开始事务。
        /// </summary>
        /// <param name="level"></param>
        public void BeginTransaction(IsolationLevel? level = null)
        {
            level ??= _options.IsolationLevel;
            _contextService.BeginTransaction(level.Value);
        }

        /// <summary>
        /// 提交事务。
        /// </summary>
        public void CommitTransaction()
        {
            _contextService.CommitTransaction();
        }

        /// <summary>
        /// 回滚事务。
        /// </summary>
        public void RollbackTransaction()
        {
            _contextService.RollbackTransaction();
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
            TryHandleConnectionTenancy(options);

            var builder = new EntityContextOptionsBuilder(options);
            OnConfiguring(builder);

            if (options.Provider == null)
            {
                throw new NotSupportedException(SR.GetString(SRKind.NotSupportDbProvider));
            }

            var contextProvider = options.GetProviderService<IContextProvider>();

            _contextService = contextProvider.CreateContextService(new ContextServiceContext(_serviceProvider, options));
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
                if (identification.ServiceProvider == null && _serviceProvider != null)
                {
                    identification.ServiceProvider = _serviceProvider;
                }
                else if (ServiceProvider == null && identification.ServiceProvider != null)
                {
                    _serviceProvider = identification.ServiceProvider;
                }
                else if (identification.ServiceProvider == null && _serviceProvider == null)
                {
                    _serviceProvider = identification.ServiceProvider = ContainerUnity.GetContainer();
                }
            }

            if (_serviceProvider == null)
            {
                _serviceProvider = ContainerUnity.GetContainer();
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
                return _contextService;
            }
            else if (serviceType == typeof(IServiceProvider))
            {
                return ServiceProvider;
            }
            else if (serviceType == typeof(IDatabase) && _contextService is IDatabaseAware aware)
            {
                return aware.Database;
            }
            else if (serviceType == typeof(IProvider))
            {
                return _contextService.Provider;
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

        /// <summary>
        /// 尝试处理多租户信息。
        /// </summary>
        /// <param name="options"></param>
        private void TryHandleConnectionTenancy(EntityContextOptions options)
        {
            var tenancyProvider = ServiceProvider.TryGetService<ITenancyProvider<ConnectionTenancyInfo>>();
            if (tenancyProvider != null)
            {
                var tenancy = new ConnectionTenancyInfo { Provider = options.Provider, ConnectionString = options.ConnectionString };
                tenancy = tenancyProvider.Resolve(tenancy);
                if (tenancy != null && tenancy.ConnectionString != options.ConnectionString)
                {
                    options.ConnectionString = tenancy.ConnectionString;
                }
            }
        }

        private void InvokeQueryPolicy(Action<IQueryPolicy> action)
        {
            if (_contextService is IQueryPolicy policy)
            {
                action(policy);
            }
            else if (_contextService is IQueryPolicyAware aware && aware.QueryPolicy != null)
            {
                action(aware.QueryPolicy);
            }
        }

        void IObjectPoolable.SetPool(string poolName)
        {
            _poolName = poolName;
        }

        void IObjectPoolable.OnRent()
        {
        }

        void IObjectPoolable.OnReturn()
        {
        }
    }
}
