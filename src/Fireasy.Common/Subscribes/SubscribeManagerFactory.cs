// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Subscribes.Configuration;

namespace Fireasy.Common.Subscribes
{
    /// <summary>
    /// 订阅管理器的工厂。
    /// </summary>
    public class SubscribeManagerFactory
    {
        /// <summary>
        /// 根据应用程序配置，创建订阅管理器。
        /// </summary>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>缺省时，如果应用程序未配置，则为 <see cref="DefaultSubscribeManager"/>，否则为配置项对应的 <see cref="ISubscribeManager"/> 实例。</returns>
        public static ISubscribeManager CreateManager(string configName = null)
        {
            ISubscribeManager manager;
            IConfigurationSettingItem setting = null;
            var section = ConfigurationUnity.GetSection<SubscribeConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                manager = ConfigurationUnity.Cached<ISubscribeManager>($"Subscribe_{configName}", () => section.Factory.CreateInstance(configName) as ISubscribeManager);
                if (manager != null)
                {
                    return manager;
                }
            }

            if (string.IsNullOrEmpty(configName))
            {
                if (section == null || (setting = section.GetDefault()) == null)
                {
                    return DefaultSubscribeManager.Instance;
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

            return ConfigurationUnity.Cached<ISubscribeManager>($"Subscribe_{configName}", () => ConfigurationUnity.CreateInstance<SubscribeConfigurationSetting, ISubscribeManager>(setting, s => s.SubscriberType));
        }
    }
}
