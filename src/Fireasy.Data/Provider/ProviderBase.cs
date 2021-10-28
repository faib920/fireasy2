// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// 基本的数据库提供者。
    /// </summary>
    public abstract class ProviderBase : IProvider
    {
        private DbProviderFactory _factory;
        private readonly IProviderFactoryResolver[] _resolvers;
        private readonly SafetyDictionary<Type, Lazy<IProviderService>> _services = new SafetyDictionary<Type, Lazy<IProviderService>>();

        /// <summary>
        /// 初始化 <see cref="ProviderBase"/> 类的新实例。
        /// </summary>
        protected ProviderBase()
        {
            InitializeServices();
        }

        /// <summary>
        /// 使用提供者名称初始化 <see cref="ProviderBase"/> 类的新实例。
        /// </summary>
        /// <param name="resolvers"></param>
        protected ProviderBase(params IProviderFactoryResolver[] resolvers)
            : this()
        {
            _resolvers = resolvers;
        }

        /// <summary>
        /// 获取描述数据库的名称。
        /// </summary>
        public virtual string ProviderName { get; set; }

        /// <summary>
        /// 获取是否有特征码。
        /// </summary>
        public virtual bool HasFeature => false;

        /// <summary>
        /// 获取数据库提供者工厂。
        /// </summary>
        public virtual DbProviderFactory DbProviderFactory
        {
            get
            {
                return SingletonLocker.Lock(ref _factory, this, () => InitDbProviderFactory());
            }
        }

        /// <summary>
        /// 获取当前连接的参数。
        /// </summary>
        /// <returns></returns>
        public abstract ConnectionParameter GetConnectionParameter(ConnectionString connectionString);

        /// <summary>
        /// 使用参数更新指定的连接。
        /// </summary>
        /// <param name="connectionString">连接字符串对象。</param>
        /// <param name="parameter"></param>
        public abstract void UpdateConnectionString(ConnectionString connectionString, ConnectionParameter parameter);

        /// <summary>
        /// 获取特征码。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public virtual string GetFeature(ConnectionString connectionString)
        {
            return string.Empty;
        }

        /// <summary>
        /// 克隆一个副本。
        /// </summary>
        /// <param name="feature">特征码。</param>
        /// <returns></returns>
        public virtual IProvider Clone(string feature)
        {
            return (IProvider)MemberwiseClone();
        }

        /// <summary>
        /// 获取注册到数据库提供者的插件服务实例。
        /// </summary>
        /// <typeparam name="TProvider">插件服务的类型。</typeparam>
        /// <returns></returns>
        public virtual TProvider GetService<TProvider>() where TProvider : class, IProviderService
        {
            if (_services.TryGetValue(typeof(TProvider), out Lazy<IProviderService> lazy))
            {
                return (TProvider)lazy.Value;
            }

            var attr = typeof(TProvider).GetCustomAttributes<DefaultProviderServiceAttribute>().FirstOrDefault();
            if (attr != null && typeof(TProvider).IsAssignableFrom(attr.ServiceType))
            {
                var service = attr.ServiceType.New<TProvider>();
                if (service == null)
                {
                    return default;
                }

                service.Provider = this;
                return service;
            }

            return default;
        }

        /// <summary>
        /// 获取注册到数据库提供者的所有插件服务。
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<IProviderService> GetServices()
        {
            return _services.Values.Select(s => s.Value);
        }

        /// <summary>
        /// 注册指定类型的插件服务。
        /// </summary>
        /// <typeparam name="TDefinition"></typeparam>
        /// <typeparam name="TImplement"></typeparam>
        public virtual IProvider RegisterService<TDefinition, TImplement>() where TDefinition : class, IProviderService where TImplement : class, TDefinition, new()
        {
            RegisterService(typeof(TDefinition), typeof(TImplement));
            return this;
        }


        /// <summary>
        /// 注册指定类型的插件服务。
        /// </summary>
        /// <param name="definedType"></param>
        /// <param name="factory"></param>
        public virtual IProvider RegisterService(Type definedType, Func<IProviderService> factory)
        {
            _services.AddOrUpdate(definedType, () => new Lazy<IProviderService>(() => Initialize(factory())));
            return this;
        }

        /// <summary>
        /// 注册插件服务类型。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        public virtual IProvider RegisterService(Type serviceType)
        {
            var providerService = serviceType.GetDirectImplementInterface(typeof(IProviderService));
            if (providerService != null)
            {
                _services.AddOrUpdate(providerService, () => new Lazy<IProviderService>(() => Initialize(serviceType.New<IProviderService>())));
            }

            return this;
        }

        /// <summary>
        /// 注册插件服务类型。
        /// </summary>
        /// <param name="providerService">服务对象。</param>
        public IProvider RegisterService(IProviderService providerService)
        {
            var providerType = providerService.GetType().GetDirectImplementInterface(typeof(IProviderService));
            if (providerType != null)
            {
                _services.AddOrUpdate(providerType, () => new Lazy<IProviderService>(() => Initialize(providerService)));
            }

            return this;
        }

        /// <summary>
        /// 注册插件服务类型。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="implType">实现类型。</param>
        public virtual IProvider RegisterService(Type serviceType, Type implType)
        {
            _services.AddOrUpdate(serviceType, () => new Lazy<IProviderService>(() => Initialize(implType.New<IProviderService>())));
            return this;
        }

        /// <summary>
        /// 处理 <see cref="DbConnection"/> 对象。
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public virtual DbConnection PrepareConnection(DbConnection connection)
        {
            return connection;
        }

        /// <summary>
        /// 处理 <see cref="DbCommand"/> 对象。
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual DbCommand PrepareCommand(DbCommand command)
        {
            return command;
        }

        /// <summary>
        /// 处理 <see cref="DbParameter"/> 对象。
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public virtual DbParameter PrepareParameter(DbParameter parameter)
        {
            return parameter;
        }

        /// <summary>
        /// 处理事务级别。
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public virtual IsolationLevel AmendIsolationLevel(IsolationLevel level)
        {
            return level;
        }

        /// <summary>
        /// 初始化 <see cref="DbProviderFactory"/> 对象。
        /// </summary>
        /// <returns></returns>
        protected virtual DbProviderFactory InitDbProviderFactory()
        {
            Exception exception = null;
            foreach (var resolver in _resolvers)
            {
                var factory = resolver.Resolve();
                if (factory != null)
                {
                    return factory;
                }

                exception = resolver.Exception;
            }

            throw exception;
        }

        /// <summary>
        /// 初始化服务。
        /// </summary>
        protected virtual void InitializeServices()
        {

        }

        private IProviderService Initialize(IProviderService service)
        {
            service.Provider = this;
            return service;
        }
    }
}
