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
using System.IO;
using System.Linq;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// 基本的数据库提供者。
    /// </summary>
    public abstract class ProviderBase : IProvider
    {
        private DbProviderFactory factory;
        private readonly IProviderFactoryResolver[] resolvers;
        private readonly SafetyDictionary<string, IProviderService> services = new SafetyDictionary<string, IProviderService>();

        /// <summary>
        /// 初始化 <see cref="ProviderBase"/> 类的新实例。
        /// </summary>
        protected ProviderBase()
        {
        }

        /// <summary>
        /// 使用提供者名称初始化 <see cref="ProviderBase"/> 类的新实例。
        /// </summary>
        /// <param name="resolvers"></param>
        protected ProviderBase(params IProviderFactoryResolver[] resolvers)
            : this()
        {
            this.resolvers = resolvers;
        }

        /// <summary>
        /// 获取描述数据库的名称。
        /// </summary>
        public virtual string ProviderName { get; set; }

        /// <summary>
        /// 获取数据库提供者工厂。
        /// </summary>
        public virtual DbProviderFactory DbProviderFactory
        {
            get { return SingletonLocker.Lock(ref factory, () => InitDbProviderFactory()); }
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
        /// 获取注册到数据库提供者的插件服务实例。
        /// </summary>
        /// <typeparam name="TService">插件服务的类型。</typeparam>
        /// <returns></returns>
        public virtual TService GetService<TService>() where TService : class, IProviderService
        {
            if (services.TryGetValue(typeof(TService).Name, out IProviderService service))
            {
                return (TService)service;
            }

            var attr = typeof(TService).GetCustomAttributes<DefaultProviderServiceAttribute>().FirstOrDefault();
            if (attr != null && typeof(TService).IsAssignableFrom(attr.ServiceType))
            {
                service = attr.ServiceType.New<TService>();
                if (service == null)
                {
                    return default;
                }

                service.Provider = this;
                return (TService)service;
            }

            return default;
        }

        /// <summary>
        /// 获取注册到数据库提供者的所有插件服务。
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<IProviderService> GetServices()
        {
            return services.Values;
        }

        /// <summary>
        /// 注册指定类型的插件服务。
        /// </summary>
        /// <typeparam name="TDefinition"></typeparam>
        /// <typeparam name="TImplement"></typeparam>
        public virtual void RegisterService<TDefinition, TImplement>() where TDefinition : class, IProviderService where TImplement : class, TDefinition, new()
        {
            RegisterService<TDefinition>(new TImplement
            {
                Provider = this
            });
        }

        /// <summary>
        /// 注册指定类型的插件服务。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="provider"></param>
        public virtual void RegisterService<TService>(TService provider) where TService : class, IProviderService
        {
            services.AddOrUpdate(typeof(TService).Name, () => provider);
        }

        /// <summary>
        /// 注册指定类型的插件服务。
        /// </summary>
        /// <param name="definedType"></param>
        /// <param name="service"></param>
        public virtual void RegisterService(Type definedType, IProviderService service)
        {
            services.AddOrUpdate(definedType.Name, () => service);
        }

        /// <summary>
        /// 注册插件服务类型。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        public virtual void RegisterService(Type serviceType)
        {
            var providerService = serviceType.GetDirectImplementInterface(typeof(IProviderService));
            if (providerService != null)
            {
                RegisterService(providerService, serviceType.New<IProviderService>());
            }
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
            foreach (var resolver in resolvers)
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
    }
}
