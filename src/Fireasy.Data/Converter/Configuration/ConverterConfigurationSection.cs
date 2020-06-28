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
    /// 表示数据转换器的配置节。对应的配置节为 fireasy/dataConverters(.net framework) 或 fireasy:dataConverters(.net core)。
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

                    if (string.IsNullOrEmpty(converterTypeName))
                    {
                        throw new Exception(SR.GetString(SRKind.NonRequiredAttribute, "converterType"));
                    }

                    return GetSetting(sourceTypeName, converterTypeName);
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

                    if (string.IsNullOrEmpty(converterTypeName))
                    {
                        throw new Exception(SR.GetString(SRKind.NonRequiredAttribute, "converterType"));
                    }

                    return GetSetting(sourceTypeName, converterTypeName);
                });
        }
#endif
        private static ConverterConfigurationSetting GetSetting(string sourceTypeName, string converterTypeName)
        {
            Type sourceType = null;
            var converterType = Type.GetType(converterTypeName, false, true);

            if (converterType == null || !typeof(IValueConverter).IsAssignableFrom(converterType))
            {
                throw new Exception(SR.GetString(SRKind.NonRequiredAttribute, "converterType"));
            }

            if (!string.IsNullOrEmpty(sourceTypeName))
            {
                sourceType = Type.GetType(sourceTypeName, false, true);
            }
            else
            {
                var baseType = converterType.BaseType;
                while (baseType != typeof(object))
                {
                    if (baseType.IsGenericType)
                    {
                        sourceType = baseType.GetGenericArguments()[0];
                        break;
                    }

                    baseType = baseType.BaseType;
                }
            }

            if (sourceType == null)
            {
                throw new Exception(SR.GetString(SRKind.NonRequiredAttribute, "sourceType"));
            }

            return new ConverterConfigurationSetting
            {
                SourceType = sourceType,
                ConverterType = converterType
            };
        }

    }
}
