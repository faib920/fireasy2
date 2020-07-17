// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Data.Configuration;
using Fireasy.Data.Extensions;
using Fireasy.Data.Provider.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// <see cref="IProvider"/> 的辅助类。
    /// </summary>
    public static class ProviderHelper
    {
        private static readonly List<ProviderWapper> _providerWappers = new List<ProviderWapper>();

        static ProviderHelper()
        {
            InitializeProviders();
        }

        /// <summary>
        /// 根据 <paramref name="setting"/> 获取对应的 <see cref="IProvider"/> 对象。
        /// </summary>
        /// <param name="setting">配置节信息。</param>
        /// <returns></returns>
        public static IProvider GetDefinedProviderInstance(IInstanceConfigurationSetting setting)
        {
            if (!string.IsNullOrEmpty(setting.ProviderName))
            {
                return GetDefinedProviderInstance(setting.ProviderName);
            }

            return GetDefinedProviderInstance(setting.ProviderType);
        }

        /// <summary>
        /// 根据 <paramref name="providerName"/> 获取对应的 <see cref="IProvider"/> 对象。
        /// </summary>
        /// <param name="providerName">提供者名称。</param>
        /// <returns></returns>
        public static IProvider GetDefinedProviderInstance(string providerName)
        {
            var wapper = _providerWappers.FirstOrDefault(s => s.Contains(providerName));
            if (wapper != null)
            {
                return wapper.LazyValue.Value;
            }

            return null;
        }

        /// <summary>
        /// 使用代码注册一个 <see cref="IProvider"/> 对象。
        /// </summary>
        /// <param name="providerName">提供者名称。</param>
        /// <param name="provider">提供者对象。</param>
        /// <returns></returns>
        public static bool RegisterProvider(string providerName, IProvider provider)
        {
            return AddProvider(providerName, provider.GetType(), () => provider);
        }

        /// <summary>
        /// 获取所提供的所有数据库提供者名称。
        /// </summary>
        /// <returns></returns>
        public static string[] GetSupportedProviders()
        {
            return _providerWappers.Select(s => s.Alias.FirstOrDefault()).ToArray();
        }

        private static bool AddProvider<T>(string providerName, Func<IProvider> factory) where T : IProvider
        {
            return AddProvider(providerName, typeof(T), factory);
        }

        private static bool AddProvider<T>(string[] providerNames, Func<IProvider> factory) where T : IProvider
        {
            return AddProvider(providerNames, typeof(T), factory);
        }

        private static bool AddProvider(string providerName, Type providerType, Func<IProvider> factory)
        {
            ProviderWapper wapper = null;
            if ((wapper = _providerWappers.FirstOrDefault(s => s.ProviderType == providerType)) != null)
            {
                wapper.Alias.Add(providerName);
                return false;
            }
            else
            {
                _providerWappers.Add(new ProviderWapper
                {
                    Alias = new List<string> { providerName },
                    ProviderType = providerType,
                    LazyValue = new Lazy<IProvider>(factory)
                });
                return true;
            }
        }

        private static bool AddProvider(string[] providerNames, Type providerType, Func<IProvider> factory)
        {
            ProviderWapper wapper = null;
            if ((wapper = _providerWappers.FirstOrDefault(s => s.ProviderType == providerType)) != null)
            {
                wapper.Alias.AddRange(providerNames);
                return false;
            }
            else
            {
                _providerWappers.Add(new ProviderWapper
                {
                    Alias = new List<string>(providerNames),
                    ProviderType = providerType,
                    LazyValue = new Lazy<IProvider>(factory)
                });
                return true;
            }
        }

        /// <summary>
        /// 初始化提供者。
        /// </summary>
        private static void InitializeProviders()
        {
            //内置的提供者
            AddProvider<OleDbProvider>("OleDb", () => OleDbProvider.Instance);
            AddProvider<OdbcProvider>("Odbc", () => OdbcProvider.Instance);
            AddProvider<MsSqlProvider>(new string[] { "SqlServer", "MsSql" }, () => MsSqlProvider.Instance);
            AddProvider<OracleProvider>("Oracle", () => OracleProvider.Instance);
            AddProvider<SQLiteProvider>("SQLite", () => SQLiteProvider.Instance);
            AddProvider<MySqlProvider>("MySql", () => MySqlProvider.Instance);
            AddProvider<PostgreSqlProvider>("PostgreSql", () => PostgreSqlProvider.Instance);
            AddProvider<FirebirdProvider>("Firebird", () => FirebirdProvider.Instance);
            AddProvider<DB2Provider>("DB2", () => DB2Provider.Instance);

            //取配置，注册自定义提供者
            var section = ConfigurationUnity.GetSection<ProviderConfigurationSection>();
            if (section != null)
            {
                RegisterCustomProviders(section);
            }
        }

        /// <summary>
        /// 使用配置注册自定义的插件服务。
        /// </summary>
        /// <param name="section">提供者的配置对象。</param>
        private static void RegisterCustomProviders(ProviderConfigurationSection section)
        {
            foreach (var key in section.Settings.Keys)
            {
                var setting = section.Settings[key];
                if (_providerWappers.Any(s => s.Contains(setting.Name)) || setting.Type == null)
                {
                    continue;
                }

                AddProvider(setting.Name, setting.Type, () =>
                {
                    var provider = setting.Type.New<IProvider>();
                    if (provider == null)
                    {
                        return null;
                    }

                    setting.ServiceTypes.ForEach(s => provider.RegisterService(s));

                    IProvider inherProvider = null;
                    if (!string.IsNullOrEmpty(setting.InheritedProvider) &&
                        (inherProvider = GetDefinedProviderInstance(setting.InheritedProvider)) != null)
                    {
                        inherProvider.GetServices().ForEach(s => provider.RegisterService(s.GetType()));
                    }
                    return provider;
                });
            }
        }
    }

    internal class ProviderWapper
    {
        internal List<string> Alias { get; set; } = new List<string>();

        internal Type ProviderType { get; set; }

        internal Lazy<IProvider> LazyValue { get; set; }

        internal bool Contains(string name)
        {
            return Alias.Any(s => s.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
