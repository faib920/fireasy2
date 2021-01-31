// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// 包装一个现有的提供者，用于重新定义 <see cref="IProviderService"/> 服务集。
    /// </summary>
    public sealed class WrappedProvider : IProvider
    {
        private readonly IProvider _inner;
        private readonly SafetyDictionary<Type, Lazy<IProviderService>> _services = new SafetyDictionary<Type, Lazy<IProviderService>>();

        public WrappedProvider(IProvider inner)
        {
            _inner = inner;
        }

        /// <summary>
        /// 获取内部的 <see cref="IProvider"/> 实例。
        /// </summary>
        public IProvider Inner => _inner;

        string IProvider.ProviderName { get => _inner.ProviderName; set => _inner.ProviderName = value; }

        bool IProvider.HasFeature => _inner.HasFeature;

        DbProviderFactory IProvider.DbProviderFactory => _inner.DbProviderFactory;

        IsolationLevel IProvider.AmendIsolationLevel(IsolationLevel level)
        {
            return _inner.AmendIsolationLevel(level);
        }

        IProvider IProvider.Clone(string feature)
        {
            return _inner.Clone(feature);
        }

        ConnectionParameter IProvider.GetConnectionParameter(ConnectionString connectionString)
        {
            return _inner.GetConnectionParameter(connectionString);
        }

        void IProvider.UpdateConnectionString(ConnectionString connectionString, ConnectionParameter parameter)
        {
            _inner.UpdateConnectionString(connectionString, parameter);
        }

        string IProvider.GetFeature(ConnectionString connectionString)
        {
            return _inner.GetFeature(connectionString);
        }

        DbCommand IProvider.PrepareCommand(DbCommand command)
        {
            return _inner.PrepareCommand(command);
        }

        DbConnection IProvider.PrepareConnection(DbConnection connection)
        {
            return _inner.PrepareConnection(connection);
        }

        DbParameter IProvider.PrepareParameter(DbParameter parameter)
        {
            return _inner.PrepareParameter(parameter);
        }

        IProvider IProvider.RegisterService(Type serviceType)
        {
            var providerService = serviceType.GetDirectImplementInterface(typeof(IProviderService));
            if (providerService != null)
            {
                _services.AddOrUpdate(providerService, () => new Lazy<IProviderService>(() => serviceType.New<IProviderService>()));
            }

            return this;
        }

        IProvider IProvider.RegisterService(IProviderService providerService)
        {
            var providerType = providerService.GetType().GetDirectImplementInterface(typeof(IProviderService));
            if (providerType != null)
            {
                _services.AddOrUpdate(providerType, () => new Lazy<IProviderService>(() => providerService));
            }

            return this;
        }

        IProvider IProvider.RegisterService(Type definedType, Func<IProviderService> factory)
        {
            _services.AddOrUpdate(definedType, () => new Lazy<IProviderService>(factory));
            return this;
        }

        TProvider IProvider.GetService<TProvider>()
        {
            if (_services.TryGetValue(typeof(TProvider), out Lazy<IProviderService> lazy))
            {
                return (TProvider)lazy.Value;
            }

            return _inner.GetService<TProvider>();
        }

        IEnumerable<IProviderService> IProvider.GetServices()
        {
            var services = _services.Values.Select(s => s.Value).ToList();

            foreach (var svr in _inner.GetServices())
            {
                var providerService = svr.GetType().GetDirectImplementInterface(typeof(IProviderService));
                if (!services.Any(s => s.GetType().GetDirectImplementInterface(typeof(IProviderService)) == providerService))
                {
                    services.Add(svr);
                }
            }

            return services;
        }
    }
}
