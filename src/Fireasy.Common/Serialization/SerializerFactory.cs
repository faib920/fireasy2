// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Serialization.Configuration;

namespace Fireasy.Common.Serialization
{
    public static class SerializerFactory
    {
        /// <summary>
        /// 根据应用程序配置，创建文本序列化器。
        /// </summary>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>缺省时，如果应用程序未配置，则为 <see cref="MemoryCacheManager"/>，否则为配置项对应的 <see cref="ICacheManager"/> 实例。</returns>
        public static ITextSerializer CreateSerializer(string configName = null)
        {
            ITextSerializer serializer;
            IConfigurationSettingItem setting = null;
            var section = ConfigurationUnity.GetSection<SerializerConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                serializer = section.Factory.CreateInstance(configName) as ITextSerializer;
                if (serializer != null)
                {
                    return serializer;
                }
            }

            if (string.IsNullOrEmpty(configName))
            {
                if (section == null || (setting = section.GetDefault()) == null)
                {
                    return null;
                }
            }
            else if (section != null)
            {
                setting = section.GetSetting(configName);
            }

            if (setting == null)
            {
                return null;
            }

            return ConfigurationUnity.CreateInstance<SerializerConfigurationSetting, ITextSerializer>(setting, s => s.SerializerType);
        }
    }
}
