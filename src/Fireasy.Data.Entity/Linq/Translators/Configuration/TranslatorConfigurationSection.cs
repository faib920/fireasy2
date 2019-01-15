// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using System.Xml;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Data.Entity.Linq.Translators.Configuration
{
    [ConfigurationSectionStorage("fireasy/dataTranslator")]
    public class TranslatorConfigurationSection : Fireasy.Common.Configuration.ConfigurationSection
    {
        public override void Initialize(XmlNode section)
        {
            var ndOption = section.SelectSingleNode("options");
            if (ndOption != null)
            {
                Options = new TranslateOptions
                    {
                        HideTableAliases = ndOption.GetAttributeValue("hideTableAliases", false),
                        HideColumnAliases = ndOption.GetAttributeValue("hideColumnAliases", false),
                        ParseCacheEnabled = ndOption.GetAttributeValue("parseCacheEnabled", true),
                        ParseCacheExpired = ndOption.GetAttributeValue("parseCacheExpired", 300),
                        DataCacheEnabled = ndOption.GetAttributeValue("dataCacheEnabled", false),
                        DataCacheExpired = ndOption.GetAttributeValue("dataCacheExpired", 60)
                    };
            }
        }

#if NETSTANDARD
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="configuration">对应的配置节点。</param>
        public override void Bind(IConfiguration configuration)
        {
            var ndOption = configuration.GetSection("options");
            if (ndOption != null)
            {
                Options = new TranslateOptions
                    {
                        HideTableAliases = ndOption.GetSection("hideTableAliases").Value.To(false),
                        HideColumnAliases = ndOption.GetSection("hideColumnAliases").Value.To(false),
                        ParseCacheEnabled = ndOption.GetSection("parseCacheEnabled").Value.To(true),
                        ParseCacheExpired = ndOption.GetSection("parseCacheExpired").Value.To(300),
                        DataCacheEnabled = ndOption.GetSection("dataCacheEnabled").Value.To(false),
                        DataCacheExpired = ndOption.GetSection("dataCacheExpired").Value.To(60)
                    };
            }
        }
#endif

        public TranslateOptions Options { get; set; }
    }
}
