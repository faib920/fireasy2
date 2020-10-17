// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Security;
using Fireasy.Data.Configuration;
using Fireasy.Data.Provider;
using Fireasy.Data.Provider.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Fireasy.Data
{
    /// <summary>
    /// <see cref="Database"/> 类的创建工厂，用于从配置或提供者类型创建一个 <see cref="Database"/> 实例。
    /// </summary>
    public static class DatabaseFactory
    {
        /// <summary>
        /// 使用配置实例名创建一个 <see cref="Database"/> 实例对象。
        /// </summary>
        /// <param name="instanceName">配置实例名称。</param>
        /// <param name="serviceProvider">检索服务提供者。</param>
        /// <returns>一个 <see cref="Database"/> 实例对象。</returns>
        public static IDatabase CreateDatabase(string instanceName = null)
        {
            var section = ConfigurationUnity.GetSection<InstanceConfigurationSection>();
            Guard.NullReference(section, SR.GetString(SRKind.NonInstanceConfigurationSection));

            var setting = string.IsNullOrEmpty(instanceName) ? section.Default : section.Settings[instanceName];
            Guard.NullReference(setting, SR.GetString(SRKind.InstanceConfigurationInvalid, instanceName));

            var database = CreateDatabase(setting);

            if (DatabaseScope.Current != null)
            {
                DatabaseScope.Current.InstanceName = instanceName;
            }

            return database.TryUseContainer();
        }

        /// <summary>
        /// 使用数据库配置实例创建一个 <see cref="Database"/> 实例对象。
        /// </summary>
        /// <param name="setting">实例配置对象。</param>
        /// <returns>一个 <see cref="Database"/> 实例对象。</returns>
        public static IDatabase CreateDatabase(IInstanceConfigurationSetting setting)
        {
            Guard.ArgumentNull(setting, nameof(setting));

            var section = ConfigurationUnity.GetSection<ProviderConfigurationSection>();
            var providerSetting = GetProviderSetting(setting, section);
            var provider = GetProvider(setting, providerSetting);

            Guard.Assert(provider != null, new Exception(SR.GetString(SRKind.ProviderNotSupported)));

            var databaseType = setting.DatabaseType;

            if (setting.Clusters.Count > 0)
            {
                var distConns = GetDistributedConnections(setting);
                return databaseType != null ? 
                    databaseType.New<IDatabase>(distConns, provider) : 
                    new ScopedDatabase(distConns, provider);
            }

            return databaseType != null ? 
                databaseType.New<IDatabase>(setting.ConnectionString, provider) : 
                new ScopedDatabase(setting.ConnectionString, provider);
        }

        /// <summary>
        /// 从 <see cref="DatabaseScope"/> 中获取 <see cref="IDatabase"/> 对象。
        /// </summary>
        /// <returns>当前线程环境中 <see cref="Database"/> 实例对象。</returns>
        public static IDatabase GetDatabaseFromScope()
        {
            if (DatabaseScope.Current == null)
            {
                return null;
            }

            return DatabaseScope.Current.Database;
        }

        /// <summary>
        /// 获取 Provider 的配置。
        /// </summary>
        /// <param name="setting">数据库实例配置对象。</param>
        /// <param name="section">提供者配置节对象。</param>
        /// <returns>一个提供者配置对象。</returns>
        private static ProviderConfigurationSetting GetProviderSetting(IInstanceConfigurationSetting setting, ProviderConfigurationSection section)
        {
            if (section == null || string.IsNullOrEmpty(setting.ProviderName))
            {
                return null;
            }

            return section.Settings[setting.ProviderName];
        }

        /// <summary>
        /// 获取 IProvidre，如果没有 setting，则获取预定义类别的 IProvider 单例。
        /// </summary>
        /// <param name="setting">数据库实例配置对象。</param>
        /// <param name="providerSetting">提供者配置对象。</param>
        /// <returns>一个提供者对象。</returns>
        private static IProvider GetProvider(IInstanceConfigurationSetting setting, ProviderConfigurationSetting providerSetting)
        {
            return providerSetting != null ? ProviderHelper.GetDefinedProviderInstance(providerSetting.Name) :
                ProviderHelper.GetDefinedProviderInstance(setting);
        }

        private static string DencryptConnection(string connectionStr)
        {
            if (connectionStr.StartsWith("XE/"))
            {
                var encryptStr = connectionStr.Substring(2);

                var len = encryptStr.Length;
                var bytes = new byte[len / 2];
                for (var i = 0; i < len; i += 2)
                {
                    bytes[i / 2] = byte.Parse(encryptStr.Substring(i, 2), NumberStyles.HexNumber);
                }

                var encrypt = CryptographyFactory.Create(CryptoAlgorithm.RC2) as SymmetricCrypto;
                return Encoding.GetEncoding(0).GetString(encrypt.Decrypt(bytes));
            }

            return connectionStr;
        }

        /// <summary>
        /// 获取分布式的数据库连接串。
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        private static List<DistributedConnectionString> GetDistributedConnections(IInstanceConfigurationSetting setting)
        {
            if (setting.Clusters.Count > 0)
            {
                var connections = new List<DistributedConnectionString>();
                foreach (var cluster in setting.Clusters)
                {
                    connections.Add(new DistributedConnectionString(cluster.ConnectionString)
                    {
                        Mode = cluster.Mode,
                        Weight = cluster.Weight
                    });
                }

                return connections;
            }

            return null;
        }
    }
}
