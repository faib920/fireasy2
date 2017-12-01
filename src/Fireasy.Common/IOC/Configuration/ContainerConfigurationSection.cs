// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Common.Ioc.Configuration
{
    /// <summary>
    /// 表示容器的配置节。无法继承此类。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/containers")]
    public sealed class ContainerConfigurationSection : ConfigurationSection<ContainerConfigurationSetting>
    {
        private string defaultInstanceName;

        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(
                section,
                "container",
                null,
                node => InitializeSetting(new ContainerConfigurationSetting
                {
                    Name = node.GetAttributeValue("name")
                }, node));

            //取默认实例
            defaultInstanceName = section.GetAttributeValue("default");

            base.Initialize(section);
        }

#if NETSTANDARD2_0
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="configuration">对应的配置节点。</param>
        public override void Bind(IConfiguration configuration)
        {
            Bind(configuration, 
                "settings", 
                func: c => InitializeSetting(c, new ContainerConfigurationSetting
                    {
                        Name = c.Key
                    }));

            defaultInstanceName = configuration.GetSection("default").Value;

            base.Bind(configuration);
        }
#endif

        /// <summary>
        /// 获取默认的配置项。
        /// </summary>
        public ContainerConfigurationSetting Default
        {
            get
            {
                if (Settings.Count == 0)
                {
                    return null;
                }

                return string.IsNullOrEmpty(defaultInstanceName) ?
                    (Settings.ContainsKey("setting0") ? Settings["setting0"] : Settings.FirstOrDefault().Value) :
                    Settings[defaultInstanceName];
            }
        }

#if NETSTANDARD2_0
        private ContainerConfigurationSetting InitializeSetting(IConfiguration config, ContainerConfigurationSetting setting)
        {
            var list = new List<RegistrationSetting>();
            foreach (var child in config.GetChildren())
            {
                var serviceType = child.GetSection("serviceType").Value;
                var componentType = child.GetSection("componentType").Value;
                var singleton = child.GetSection("singleton").Value.To<bool>();

                var assembly = child.GetSection("assembly").Value;

                if (!string.IsNullOrEmpty(assembly))
                {
                    ResolveAssembly(assembly, singleton, list);
                    continue;
                }

                if (string.IsNullOrEmpty(serviceType) ||
                    string.IsNullOrEmpty(componentType))
                {
                    continue;
                }

                list.Add(new RegistrationSetting
                {
                    ServiceType = serviceType.ParseType(),
                    ComponentType = componentType.ParseType(),
                    Singleton = singleton
                });

            }

            setting.Registrations = list.ToReadOnly();
            return setting;
        }
#endif

        private ContainerConfigurationSetting InitializeSetting(ContainerConfigurationSetting setting, XmlNode node)
        {
            var list = new List<RegistrationSetting>();
            foreach (XmlNode child in node.SelectNodes("registration"))
            {
                var serviceType = child.GetAttributeValue("serviceType");
                var componentType = child.GetAttributeValue("componentType");
                var singleton = child.GetAttributeValue<bool>("singleton");

                var assembly = child.GetAttributeValue("assembly");

                if (!string.IsNullOrEmpty(assembly))
                {
                    ResolveAssembly(assembly, singleton, list);
                    continue;
                }

                if (string.IsNullOrEmpty(serviceType) ||
                    string.IsNullOrEmpty(componentType))
                {
                    continue;
                }

                list.Add(new RegistrationSetting
                {
                    ServiceType = serviceType.ParseType(),
                    ComponentType = componentType.ParseType(),
                    Singleton = singleton
                });
            }

            setting.Registrations = list.ToReadOnly();
            return setting;
        }

        private void ResolveAssembly(string assemblyName, bool singleton, List<RegistrationSetting> list)
        {
            var assembly = Assembly.Load(assemblyName);
            foreach (var type in assembly.GetExportedTypes())
            {
                if (type.IsInterface || type.IsEnum)
                {
                    continue;
                }

                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsDefined<IgnoreRegisterAttribute>())
                    {
                        continue;
                    }

                    list.Add(new RegistrationSetting
                    {
                        ServiceType = interfaceType,
                        ComponentType = type,
                        Singleton = singleton
                    });
                }
            }
        }
    }
}
