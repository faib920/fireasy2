// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Configuration;
using System;

namespace Fireasy.Common.Logging.Configuration
{
    /// <summary>
    /// 日志配置类。
    /// </summary>
    public class LoggingConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置配置名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置 <see cref="ILogger"/> 的实例类型。
        /// </summary>
        public Type LogType { get; set; }
    }
}
