// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using Fireasy.Common.Configuration;

namespace Fireasy.Common.Composition.Configuration
{
    /// <summary>
    /// MEF 导入的配置。
    /// </summary>
    public class ImportConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置配置名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置协定接口类型。
        /// </summary>
        public Type ContractType { get; set; }

        /// <summary>
        /// 获取或设置导入的类型。
        /// </summary>
        public Type ImportType { get; set; }

        /// <summary>
        /// 获取或设置程序集名称。
        /// </summary>
        public string Assembly { get; set; }
    }
}
