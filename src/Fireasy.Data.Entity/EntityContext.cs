// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Configuration;
using Fireasy.Data.Configuration;
using Fireasy.Data.Entity.Linq;
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
    public abstract class EntityContext : IDisposable, IServiceProvider
    {
        private EntityContextOptions options;
        private bool isDisposed;
        private IContextService service;

        /// <summary>
        /// 初始化 <see cref="EntityContext"/> 类的新实例。
        /// </summary>
        public EntityContext()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// 使用一个配置名称来初始化 <see cref="EntityContext"/> 类的新实例。
        /// </summary>
        /// <param name="instanceName">实例名称。</param>
        public EntityContext(string instanceName)
            : this(new EntityContextOptions(instanceName))
        {
        }

        /// <summary>
        /// 初始化 <see cref="EntityContext"/> 类的新实例。
        /// </summary>
        /// <param name="options">选项参数。</param>
        public EntityContext(EntityContextOptions options)
        {
            this.options = options;

            OnConfiguring(new EntityContextOptionsBuilder(options));
            Initialize(options);

            new EntityRepositoryDiscoveryService(this, options).InitializeSets();
        }

        /// <summary>
        /// 析构函数。
        /// </summary>
        ~EntityContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// 获取关联的 <see cref="IDatabase"/> 对象。
        /// </summary>
        public IDatabase Database
        {
            get
            {
                return service?.Database;
            }
        }

        /// <summary>
        /// 销毁资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 销毁资源。
        /// </summary>
        /// <param name="disposing">如果为 true，则同时释放托管资源和非托管资源；如果为 false，则仅释放非托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                service?.Dispose();
                isDisposed = true;
            }
        }

        /// <summary>
        /// 为指定的类型返回 <see cref="EntityRepository{TEntity}"/>
        /// </summary>
        /// <typeparam name="TEntity">实体类型。</typeparam>
        /// <returns></returns>
        public IRepository<TEntity> Set<TEntity>() where TEntity : IEntity
        {
            return (IRepository<TEntity>)service.GetDbSet(typeof(TEntity));
        }

        /// <summary>
        /// 为指定的类型返回 <see cref="IRepository"/>
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <returns></returns>
        public IRepository Set(Type entityType)
        {
            return service.GetDbSet(entityType);
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
            if (service is IQueryPolicy policy)
            {
                policy.IncludeWith(fnMember);
            }

            return this;
        }

        /// <summary>
        /// 对关联对象
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="memberQuery"></param>
        /// <returns></returns>
        public EntityContext Associate<TEntity>(Expression<Func<TEntity, IEnumerable>> memberQuery) where TEntity : IEntity
        {
            if (service is IQueryPolicy policy)
            {
                policy.AssociateWith(memberQuery);
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="fnApply"></param>
        /// <returns></returns>
        public EntityContext Apply<TEntity>(Expression<Func<IEnumerable<TEntity>, IEnumerable<TEntity>>> fnApply) where TEntity : IEntity
        {
            if (service is IQueryPolicy policy)
            {
                policy.Apply(fnApply);
            }

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
            if (service is IQueryPolicy policy)
            {
                policy.Apply(entityType, fnApply);
            }

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
        public void BeginTransaction(IsolationLevel level = IsolationLevel.ReadUncommitted)
        {
            service.BeginTransaction(level);
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

            EntityContextInitializeContext initContext = null;

            if (options.ContextFactory != null)
            {
                initContext = options.ContextFactory();
            }
            else
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

                    initContext = new EntityContextInitializeContext(options, ProviderHelper.GetDefinedProviderInstance(setting), setting.ConnectionString);
                }
            }

            if (initContext == null || initContext.Provider == null)
            {
                throw new InvalidOperationException(SR.GetString(SRKind.MustAssignEntityContextInitializeContext));
            }

            initContext.Options = options;

            var provider = initContext.Provider.GetService<IContextProvider>();
            service = provider.CreateContextService(initContext);
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
            else if (serviceType == typeof(IDatabase))
            {
                return service.Database;
            }
            else if (serviceType == typeof(IProvider))
            {
                return service.InitializeContext.Provider;
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
    }
}
