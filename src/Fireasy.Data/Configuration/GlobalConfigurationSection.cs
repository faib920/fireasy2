// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Xml;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;

namespace Fireasy.Data.Configuration
{
    [ConfigurationSectionStorage("fireasy/dataGlobal")]
    public class GlobalConfigurationSection : ConfigurationSection
    {
        public override void Initialize(XmlNode section)
        {
            var ndOption = section.SelectSingleNode("options");
            if (ndOption != null)
            {
                Options = new GlobalOptions
                    {
                        AttachQuote = ndOption.GetAttributeValue("attachQuote", false)
                    };
            }
        }

        public GlobalOptions Options { get; set; }
    }
}
