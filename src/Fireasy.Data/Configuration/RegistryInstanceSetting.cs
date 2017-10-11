// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Xml;
using Fireasy.Common;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using Fireasy.Data.Provider;
using Microsoft.Win32;

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 一个提供数据库字符串配置的类，使用系统注册表进行配置。
    /// </summary>
    [Serializable]
    public sealed class RegistryInstanceSetting : IInstanceConfigurationSetting
    {
        /// <summary>
        /// 返回提供者配置名称。
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// 获取实例名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 返回数据库类型。
        /// </summary>
        public string ProviderType { get; set; }

        /// <summary>
        /// 返回数据库类型。
        /// </summary>
        public Type DatabaseType { get; set; }

        /// <summary>
        /// 返回数据库连接字符串。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 获取根键名称。
        /// </summary>
        public string RootKey { get; set; }

        /// <summary>
        /// 获取子键名称。
        /// </summary>
        public string SubKey { get; set; }

        /// <summary>
        /// 获取值的名称。
        /// </summary>
        public string ValueKey { get; set; }

        /// <summary>
        /// 将注册表根键名转换成相应的 <see cref="RegistryKey"/> 对象。
        /// </summary>
        /// <param name="rootKey"></param>
        /// <returns></returns>
        public static RegistryKey GetRootKey(string rootKey)
        {
            if (rootKey.Equals("hcu", StringComparison.OrdinalIgnoreCase))
            {
                return Registry.CurrentUser;
            }
            if (rootKey.Equals("hlm", StringComparison.OrdinalIgnoreCase))
            {
                return Registry.LocalMachine;
            }
            if (rootKey.Equals("hcc", StringComparison.OrdinalIgnoreCase))
            {
                return Registry.CurrentConfig;
            }
            if (rootKey.Equals("hcr", StringComparison.OrdinalIgnoreCase))
            {
                return Registry.ClassesRoot;
            }
            if (rootKey.Equals("hu", StringComparison.OrdinalIgnoreCase))
            {
                return Registry.Users;
            }
            try
            {
                var regRoot = typeof(Registry).GetField(rootKey).GetValue(null) as RegistryKey;
                return regRoot;
            }
            catch
            {
                return null;
            }
        }

        internal class SettingParseHandler : IConfigurationSettingParseHandler
        {
            public IConfigurationSettingItem Parse(XmlNode node)
            {
                var setting = new RegistryInstanceSetting();
                setting.Name = node.GetAttributeValue("name");
                setting.RootKey = node.GetAttributeValue("rootKey");
                setting.SubKey = node.GetAttributeValue("subKey");
                setting.ValueKey = node.GetAttributeValue("valueKey");

                if (string.IsNullOrEmpty(setting.RootKey) ||
                    string.IsNullOrEmpty(setting.SubKey) ||
                    string.IsNullOrEmpty(setting.ValueKey))
                {
                    ThrowRegistryInvalid();
                }
                var regRoot = GetRootKey(setting.RootKey);
                if (regRoot == null)
                {
                    ThrowRegistryInvalid();
                }

                var srKey = regRoot.OpenSubKey(setting.SubKey);
                if (srKey == null)
                {
                    ThrowRegistryInvalid();
                }
                var regData = srKey.GetValue(setting.ValueKey);
                srKey.Close();
                Guard.NullReference(regData, "regData");

                if (ParseRegistryData(setting, regData))
                {
                    return setting;
                }
                return null;
            }

            private void ThrowRegistryInvalid()
            {
                throw new InvalidOperationException(SR.GetString(SRKind.RegistryInvalid));
            }

            private bool ParseRegistryData(RegistryInstanceSetting setting, object data)
            {
                if (data is byte[])
                {
                    try
                    {
                        //反序列化
                        //var header = new SerializeHeader { HeaderBytes = Encoding.ASCII.GetBytes(Name) };
                        var store = new BinaryCompressSerializer().Deserialize<BinaryConnectionStore>(data as byte[]);
                        if (store != null)
                        {
                            if (!string.IsNullOrEmpty(store.DatabaseType))
                            {
                                setting.DatabaseType = Type.GetType(store.DatabaseType, false, true);
                            }

                            setting.ProviderType = store.ProviderType;
                            setting.ConnectionString = ConnectionStringHelper.GetConnectionString(store.ConnectionString);
                            return true;
                        }
                    }
                    catch
                    {
                        throw new InvalidOperationException(SR.GetString(SRKind.FailInDataParse));
                    }
                }
                else if (data is string)
                {
                    var con = new ConnectionString(data.ToString());
                    setting.ProviderType = con.ProviderType;
                    setting.DatabaseType = Type.GetType(con.DatabaseType, false, true);
                    setting.ConnectionString = con.ToString();
                    return true;
                }
                return false;
            }
        }
    }
}
