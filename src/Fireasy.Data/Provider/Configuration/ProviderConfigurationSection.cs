// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Xml;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Data.Provider.Configuration
{
    /// <summary>
    /// 表示数据库提供者的配置节。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/dataProviders")]
    public sealed class ProviderConfigurationSection : ConfigurationSection<ProviderConfigurationSetting>
    {
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(section, "provider", null, ParseProviderSetting);
        }

#if NETSTANDARD
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="configuration">对应的配置节点。</param>
        public override void Bind(IConfiguration configuration)
        {
            Bind(configuration, "settings", null, ParseProviderSetting);
        }
#endif

        private ProviderConfigurationSetting ParseProviderSetting(XmlNode node)
        {
            return new ProviderConfigurationSetting(LoadServices(node))
            {
                Name = node.GetAttributeValue("name"),
                InheritedProvider = node.GetAttributeValue("inherited"),
                Type = Type.GetType(GetEllipticalTypeName(node.GetAttributeValue("type")), false, true)
            };
        }

        /// <summary>
        /// 循环节点provider\service，将服务类添加到配置的服务集合中。
        /// </summary>
        /// <param name="section">provider 节点。</param>
        private IEnumerable<Type> LoadServices(XmlNode section)
        {
            var types = new List<Type>();
            section.EachChildren("services", node =>
                {
                    var type = Type.GetType(GetEllipticalTypeName(node.GetAttributeValue("type")), false, true);
                    if (type != null)
                    {
                        types.Add(type);
                    }
                });
            return types;
        }

#if NETSTANDARD
        private ProviderConfigurationSetting ParseProviderSetting(IConfiguration configuration)
        {
            return new ProviderConfigurationSetting(LoadServices(configuration))
                {
                    Name = ((Microsoft.Extensions.Configuration.IConfigurationSection)configuration).Key,
                    InheritedProvider = configuration.GetSection("inherited").Value,
                    Type = Type.GetType(GetEllipticalTypeName(configuration.GetSection("type").Value), false, true)
                };
        }

        /// <summary>
        /// 循环节点provider\service，将服务类添加到配置的服务集合中。
        /// </summary>
        /// <param name="configuration">provider 节点。</param>
        private IEnumerable<Type> LoadServices(IConfiguration configuration)
        {
            var types = new List<Type>();
            foreach (var child in configuration.GetSection("services").GetChildren())
            {
                var typeName = GetEllipticalTypeName(child.GetSection("type").Value);
                if (string.IsNullOrEmpty(typeName))
                {
                    continue;
                }

                var type = Type.GetType(typeName, false, true);
                if (type != null)
                {
                    types.Add(type);
                }
            }
            return types;
        }
#endif

        private string GetEllipticalTypeName(string typeName)
        {
            if (!string.IsNullOrEmpty(typeName) && !typeName.Contains(","))
            {
                typeName = string.Format("Fireasy.Data.Provider.{0}, Fireasy.Data", typeName);
            }

            return typeName;
        }
    }
}
