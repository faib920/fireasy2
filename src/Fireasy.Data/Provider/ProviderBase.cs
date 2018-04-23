// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// 基本的数据库提供者。
    /// </summary>
    public abstract class ProviderBase : IProvider
    {
        private DbProviderFactory factory;
        private readonly string providerName;
        private Dictionary<string, IProviderService> services = new Dictionary<string, IProviderService>();

        /// <summary>
        /// 初始化 <see cref="ProviderBase"/> 类的新实例。
        /// </summary>
        protected ProviderBase()
        {
        }

        /// <summary>
        /// 使用提供者名称初始化 <see cref="ProviderBase"/> 类的新实例。
        /// </summary>
        /// <param name="providerName"></param>
        protected ProviderBase(string providerName)
            : this()
        {
            this.providerName = providerName;
        }

        /// <summary>
        /// 获取描述数据库的名称。
        /// </summary>
        public abstract string DbName { get; }

        /// <summary>
        /// 获取数据库提供者工厂。
        /// </summary>
        public virtual DbProviderFactory DbProviderFactory
        {
            get { return factory ?? (factory = InitDbProviderFactory()); }
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
        /// <returns></returns>
        public abstract string UpdateConnectionString(ConnectionString connectionString, ConnectionParameter parameter);

        /// <summary>
        /// 获取注册到数据库提供者的插件服务实例。
        /// </summary>
        /// <typeparam name="TProvider">插件服务的类型。</typeparam>
        /// <returns></returns>
        public virtual TProvider GetService<TProvider>() where TProvider : class, IProviderService
        {
            IProviderService service;
            if (services.TryGetValue(typeof(TProvider).Name, out service))
            {
                return (TProvider)service;
            }

            return null;
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
        /// <typeparam name="TDefined"></typeparam>
        /// <typeparam name="TImplement"></typeparam>
        public virtual void RegisterService<TDefined, TImplement>() where TDefined : class, IProviderService where TImplement : class, TDefined, new()
        {
            var instance = new TImplement();
            instance.Provider = this;
            RegisterService<TDefined>(instance);
        }

        /// <summary>
        /// 注册指定类型的插件服务。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="provider"></param>
        public virtual void RegisterService<TService>(TService provider) where TService : class, IProviderService
        {
            lock (services)
            {
                services.AddOrReplace(typeof(TService).Name, provider);
            }
        }

        /// <summary>
        /// 注册指定类型的插件服务。
        /// </summary>
        /// <param name="definedType"></param>
        /// <param name="service"></param>
        public virtual void RegisterService(Type definedType, IProviderService service)
        {
            lock (services)
            {
                services.AddOrReplace(definedType.Name, service);
            }
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
        /// 初始化 <see cref="DbProviderFactory"/> 对象。
        /// </summary>
        /// <returns></returns>
        protected virtual DbProviderFactory InitDbProviderFactory()
        {
#if !NETSTANDARD2_0
            return DbProviderFactories.GetFactory(providerName);
#else
            return null;
#endif
        }
    }
}
