// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using System;
using System.Collections.Generic;

namespace Fireasy.Common.Logging.Configuration
{
    [ConfigurationSettingParseType(typeof(ComplexLoggingSettingParseHandler))]
    public class ComplexLoggingSetting : LoggingConfigurationSetting
    {
        public List<ComplexLoggingSettingPair> Pairs { get; } = new List<ComplexLoggingSettingPair>();
    }

    public class ComplexLoggingSettingPair
    {
        /// <summary>
        /// 获取日志级别。
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// 获取或设置 <see cref="ILogger"/> 的实例类型。
        /// </summary>
        public Type LogType { get; set; }
    }
}
