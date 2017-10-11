// <copyright company="Faib Studio"
//      email="faib920@126.com"
//      qq="55570729"
//      date="2011-2-16">
//   (c) Copyright Faib Studio 2011. All rights reserved.
// </copyright>
// ---------------------------------------------------------------

using System;
using System.IO;
using System.Xml;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using Fireasy.Data.Provider;

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 一个提供数据库字符串配置的类，使用二进制文件进行配置。
    /// </summary>
    [Serializable]
    public sealed class BinaryInstanceSetting : IInstanceConfigurationSetting
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
        /// 获取数据提供者类型。
        /// </summary>
        public string ProviderType { get; set; }

        /// <summary>
        /// 返回数据库类型。
        /// </summary>
        public Type DatabaseType { get; set; }

        /// <summary>
        /// 获取数据库连接字符串。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 返回二进制文件名称。
        /// </summary>
        public string FileName { get; set; }

        internal class SettingParseHandler : IConfigurationSettingParseHandler
        {
            public IConfigurationSettingItem Parse(XmlNode node)
            {
                var setting = new BinaryInstanceSetting();

                var file = node.GetAttributeValue("fileName");
                DbUtility.ParseDataDirectory(ref file);
                setting.FileName = file;
                if (!File.Exists(setting.FileName))
                {
                    throw new FileNotFoundException(SR.GetString(SRKind.FileNotFound, setting.FileName), setting.FileName);
                }

                FileStream stream = null;
                try
                {
                    //读取文件
                    stream = new FileStream(setting.FileName, FileMode.Open, FileAccess.Read);
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    stream.Close();
                    stream = null;

                    //反序列化
                    var store = new BinaryCompressSerializer().Deserialize<BinaryConnectionStore>(bytes);
                    if (store != null)
                    {
                        setting.ProviderType = store.ProviderType;
                        setting.DatabaseType = Type.GetType(store.DatabaseType, false, true);
                        setting.ConnectionString = ConnectionStringHelper.GetConnectionString(store.ConnectionString);
                    }
                }
                catch
                {
                    throw new InvalidOperationException(SR.GetString(SRKind.FailInDataParse));
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                }

                return setting;
            }
        }
    }
}
