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
using System;
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
                        CacheParsing = ndOption.GetAttributeValue("cacheParsing", true),
                        CacheParsingTimes = ndOption.GetAttributeValue("cacheParsingTimes").ToTimeSpan(TimeSpan.FromMinutes(10)),
                        CacheExecution = ndOption.GetAttributeValue("cacheExecution", false),
                        CacheExecutionTimes = ndOption.GetAttributeValue("cacheExecutionTimes").ToTimeSpan(TimeSpan.FromMinutes(5)),
                        TraceEntityState = ndOption.GetAttributeValue("traceEntityState", true),
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
                        CacheParsing = ndOption.GetSection("cacheParsing").Value.To(true),
                        CacheParsingTimes = ndOption.GetSection("cacheParsingTimes").Value.ToTimeSpan(TimeSpan.FromMinutes(10)),
                        CacheExecution = ndOption.GetSection("cacheExecution").Value.To(false),
                        CacheExecutionTimes = ndOption.GetSection("cacheExecutionTimes").Value.ToTimeSpan(TimeSpan.FromMinutes(5)),
                        TraceEntityState = ndOption.GetSection("traceEntityState").Value.To(true)
                };
            }
        }
#endif

        public TranslateOptions Options { get; set; }
    }
}
