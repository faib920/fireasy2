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
using System;
using System.Diagnostics;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Common.Ioc.Configuration
{
    /// <summary>
    /// 表示容器的配置节。无法继承此类。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/containers")]
    public sealed class ContainerConfigurationSection : DefaultInstaneConfigurationSection<ContainerConfigurationSetting>
    {
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
            DefaultInstanceName = section.GetAttributeValue("default");

            base.Initialize(section);
        }

#if NETSTANDARD
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

            DefaultInstanceName = configuration.GetSection("default").Value;

            base.Bind(configuration);
        }

        private ContainerConfigurationSetting InitializeSetting(IConfiguration config, ContainerConfigurationSetting setting)
        {
            var list = new List<RegistrationSetting>();
            foreach (var child in config.GetChildren())
            {
                var serviceType = child.GetSection("serviceType").Value;
                var componentType = child.GetSection("componentType").Value;
                if (string.IsNullOrEmpty(componentType))
                {
                    componentType = child.GetSection("implementationType").Value;
                }

                if (string.IsNullOrEmpty(componentType))
                {
                    componentType = serviceType;
                }

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
                    ImplementationType = componentType.ParseType(),
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
                if (string.IsNullOrEmpty(componentType))
                {
                    componentType = child.GetAttributeValue("implementationType");
                }

                if (string.IsNullOrEmpty(componentType))
                {
                    componentType = serviceType;
                }

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
                    ImplementationType = componentType.ParseType(),
                    Singleton = singleton
                });
            }

            setting.Registrations = list.ToReadOnly();
            return setting;
        }

        private void ResolveAssembly(string assemblyName, bool singleton, List<RegistrationSetting> list)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    list.Add(new RegistrationSetting
                    {
                        Assembly = assembly,
                        Singleton = singleton
                    });
                }
            }
            catch
            {
                Trace.WriteLine($"Load assembly {assemblyName} failed.");
            }
        }
    }
}
