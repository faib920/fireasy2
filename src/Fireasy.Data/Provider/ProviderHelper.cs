// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.ComponentModel;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using Fireasy.Data.Provider.Configuration;
using System;
using System.Linq;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// <see cref="IProvider"/> 的辅助类。
    /// </summary>
    public static class ProviderHelper
    {
        private static readonly SafetyDictionary<string, IProvider> dicProviders = new SafetyDictionary<string, IProvider>();

        static ProviderHelper()
        {
            InitializeProviders();
        }

        /// <summary>
        /// 根据 <paramref name="providerName"/> 获取对应的 <see cref="IProvider"/> 对象。
        /// </summary>
        /// <param name="providerName">提供者名称。</param>
        /// <returns></returns>
        public static IProvider GetDefinedProviderInstance(string providerName)
        {
            var provider = dicProviders.FirstOrDefault(s => s.Key.Equals(providerName, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(provider.Key) && provider.Value != null)
            {
                return provider.Value;
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
            return dicProviders.TryAdd(providerName, provider);
        }

        /// <summary>
        /// 获取所提供的所有数据库提供者名称。
        /// </summary>
        /// <returns></returns>
        public static string[] GetSupportedProviders()
        {
            return dicProviders.Select(s => s.Key).ToArray();
        }

        /// <summary>
        /// 初始化提供者。
        /// </summary>
        private static void InitializeProviders()
        {
            //内置的提供者
#if !NETSTANDARD
            dicProviders.TryAdd("OleDb", OleDbProvider.Instance);
            dicProviders.TryAdd("Odbc", OdbcProvider.Instance);
#endif
            dicProviders.TryAdd("MsSql", MsSqlProvider.Instance);
            dicProviders.TryAdd("Oracle", OracleProvider.Instance);
            dicProviders.TryAdd("SQLite", SQLiteProvider.Instance);
            dicProviders.TryAdd("MySql", MySqlProvider.Instance);
            dicProviders.TryAdd("PostgreSql", PostgreSqlProvider.Instance);
            dicProviders.TryAdd("Firebird", FirebirdProvider.Instance);
            dicProviders.TryAdd("DB2", DB2Provider.Instance);

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
                if (dicProviders.ContainsKey(setting.Name))
                {
                    continue;
                }

                var provider = setting.Type.New<IProvider>();
                if (provider == null)
                {
                    continue;
                }

                //为提供者注册插件服务
                setting.ServiceTypes.ForEach(s => provider.RegisterService(s));

                RegisterProvider(setting.Name, provider);
            }
        }
    }
}
