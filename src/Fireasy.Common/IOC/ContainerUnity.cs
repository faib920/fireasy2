// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.Configuration;
using Fireasy.Common.Ioc.Configuration;

namespace Fireasy.Common.Ioc
{
    /// <summary>
    /// IOC 容器的管理单元。
    /// </summary>
    public static class ContainerUnity
    {
        private const string DEFAULT = "_default_container";

        /// <summary>
        /// 获取指定名称的 IOC 容器，如果该容器不存在，则创建新的容器。<paramref name="name"/> 为 null 时返回 <see cref="Container.Instance"/> 实例。
        /// 该方法将使用 <paramref name="name"/> 在 <see cref="ContainerConfigurationSection"/> 中检索被映射的反转定义，将服务类型登记到容器中。
        /// </summary>
        /// <param name="name">用于标记容器的名称。</param>
        /// <returns></returns>
        public static Container GetContainer(string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = DEFAULT;
            }

            var cacheMgr = MemoryCacheManager.Instance;
            return cacheMgr.TryGet<Container>(name, () =>
                {
                    var container = new Container();
                    ConfigureContainer(name, container);
                    return container;
                }, () => NeverExpired.Instance);
        }

        /// <summary>
        /// 从路径的多个配置文件中读取容器。
        /// </summary>
        /// <param name="path">存放路径。</param>
        /// <param name="pattern">配置文件的通配符。</param>
        /// <returns></returns>
        public static Container GetContainer(string path, string pattern)
        {
            var cacheMgr = MemoryCacheManager.Instance;
            return cacheMgr.TryGet<Container>(path + "/" + pattern, () =>
                {
                    var container = new Container();
                    return container.Config(path, pattern);
                }, () => NeverExpired.Instance);
        }

        private static void ConfigureContainer(string name, Container container)
        {
            var section = ConfigurationUnity.GetSection<ContainerConfigurationSection>();

            ContainerConfigurationSetting setting;

            if (section == null)
            {
                return;
            }

            if (name == DEFAULT && section.Default != null)
            {
                setting = section.Default;
            }
            else if ((setting = section.Settings[name]) == null)
            {
                return;
            }

            foreach (var reg in setting.Registrations)
            {
                if (reg.ServiceType != null && reg.ComponentType != null)
                {
                    container.Register(reg.ServiceType, reg.ComponentType, reg.Singleton ? Lifetime.Singleton : Lifetime.Transient);
                }
            }
        }
    }
}
