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
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Data.Configuration
{
    [ConfigurationSectionStorage("fireasy/dataGlobal")]
    public class GlobalConfigurationSection : Fireasy.Common.Configuration.ConfigurationSection
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

#if NETSTANDARD2_0
        public override void Bind(IConfiguration configuration)
        {
            var ndOption = configuration.GetSection("options");
            if (ndOption != null)
            {
                Options = new GlobalOptions
                    {
                        AttachQuote = ndOption.GetSection("attachQuote").Value.To(false)
                    };
            }
        }
#endif

        public GlobalOptions Options { get; set; }
    }
}
