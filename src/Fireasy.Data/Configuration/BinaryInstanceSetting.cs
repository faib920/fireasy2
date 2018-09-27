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
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 一个提供数据库字符串配置的类，使用二进制文件进行配置。
    /// </summary>
    [Serializable]
    public sealed class BinaryInstanceSetting : DefaultInstanceConfigurationSetting
    {
        /// <summary>
        /// 返回二进制文件名称。
        /// </summary>
        public string FileName { get; set; }

        internal class SettingParseHandler : IConfigurationSettingParseHandler
        {
            public IConfigurationSettingItem Parse(XmlNode node)
            {
                var fileName = node.GetAttributeValue("fileName");
                return Parse(fileName);
            }

#if NETSTANDARD2_0
            public IConfigurationSettingItem Parse(IConfiguration configuration)
            {
                var fileName = configuration.GetSection("fileName").Value;
                return Parse(fileName);
            }
#endif
            private IConfigurationSettingItem Parse(string fileName)
            {
                var setting = new BinaryInstanceSetting();

                fileName = DbUtility.ResolveFullPath(fileName);
                setting.FileName = fileName;
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
