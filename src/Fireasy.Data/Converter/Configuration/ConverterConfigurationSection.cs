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

namespace Fireasy.Data.Converter.Configuration
{
    /// <summary>
    /// 表示数据转换器的配置节。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/dataConverters")]
    public sealed class ConverterConfigurationSection : ConfigurationSection<ConverterConfigurationSetting>
    {
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(section, "converter", null, node =>
                {
                    var sourceTypeName = node.GetAttributeValue("sourceType");
                    var converterTypeName = node.GetAttributeValue("converterType");
                    if (string.IsNullOrEmpty(sourceTypeName))
                    {
                        throw new Exception(SR.GetString(SRKind.NonRequiredAttribute, "sourceTypeName"));
                    }

                    if (string.IsNullOrEmpty(converterTypeName))
                    {
                        throw new Exception(SR.GetString(SRKind.NonRequiredAttribute, "converterType"));
                    }

                    return new ConverterConfigurationSetting
                               {
                                   SourceType = Type.GetType(sourceTypeName, false, true),
                                   ConverterType = Type.GetType(converterTypeName, false, true)
                               };
                });
        }

#if NETSTANDARD
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="configuration">对应的配置节点。</param>
        public override void Bind(IConfiguration configuration)
        {
            Bind(configuration, "settings", null, c =>
                {
                    var sourceTypeName = c.GetSection("sourceType").Value;
                    var converterTypeName = c.GetSection("converterType").Value;
                    if (string.IsNullOrEmpty(sourceTypeName))
                    {
                        throw new Exception(SR.GetString(SRKind.NonRequiredAttribute, "sourceTypeName"));
                    }

                    if (string.IsNullOrEmpty(converterTypeName))
                    {
                        throw new Exception(SR.GetString(SRKind.NonRequiredAttribute, "converterType"));
                    }

                    return new ConverterConfigurationSetting
                        {
                            SourceType = Type.GetType(sourceTypeName, false, true),
                            ConverterType = Type.GetType(converterTypeName, false, true)
                        };
                });
        }
#endif

    }
}
