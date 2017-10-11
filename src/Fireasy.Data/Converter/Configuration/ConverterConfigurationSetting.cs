// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Fireasy.Common.Configuration;

namespace Fireasy.Data.Converter.Configuration
{
    /// <summary>
    /// 转换器的配置信息。
    /// </summary>
    public class ConverterConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置数据源的类型。
        /// </summary>
        public Type SourceType { get; set; }

        /// <summary>
        /// 获取或设置转换器的类型。
        /// </summary>
        public Type ConverterType { get; set; }
    }
}
