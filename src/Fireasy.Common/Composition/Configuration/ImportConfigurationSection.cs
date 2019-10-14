// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Xml;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Common.Composition.Configuration
{
    /// <summary>
    /// 表示 MEF 的导入配置节。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/imports")]
    public sealed class ImportConfigurationSection : ConfigurationSection<ImportConfigurationSetting>
    {
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(
                section, 
                "import", 
                null, 
                node => 
                new ImportConfigurationSetting
                    {
                        Assembly = node.GetAttributeValue("assembly"),
                        ContractType = Type.GetType(node.GetAttributeValue("contractType"), false, true),
                        ImportType = Type.GetType(node.GetAttributeValue("importType"), false, true)
                    });
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
                func: c => new ImportConfigurationSetting
                {
                    Assembly = c.GetSection("assembly").Value,
                    ContractType = Type.GetType(c.GetSection("contractType").Value??string.Empty, false, true),
                    ImportType = Type.GetType(c.GetSection("importType").Value ?? string.Empty, false, true)
                });

            base.Bind(configuration);
        }
#endif
    }
}
