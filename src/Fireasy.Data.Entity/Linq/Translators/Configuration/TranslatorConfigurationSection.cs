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

namespace Fireasy.Data.Entity.Linq.Translators.Configuration
{
    [ConfigurationSectionStorage("fireasy/dataTranslator")]
    public class TranslatorConfigurationSection : ConfigurationSection
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

        public TranslateOptions Options { get; set; }
    }
}
