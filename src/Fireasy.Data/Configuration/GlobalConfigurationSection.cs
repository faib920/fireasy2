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

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 公共配置参数。对应的配置节为 fireasy/dataGlobal(.net framework) 或 fireasy:dataGlobal(.net core)。
    /// </summary>
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

#if NETSTANDARD
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
