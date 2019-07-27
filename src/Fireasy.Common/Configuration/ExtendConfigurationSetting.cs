// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>

// -----------------------------------------------------------------------
namespace Fireasy.Common.Configuration
{
    /// <summary>
    /// 表示受扩展的配置，包括一个基本配置及扩展配置。
    /// </summary>
    internal class ExtendConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置基本的配置对象。
        /// </summary>
        public IConfigurationSettingItem Base { get; set; }

        /// <summary>
        /// 获取或设置扩展的配置对象。
        /// </summary>
        public IConfigurationSettingItem Extend { get; set; }
    }
}
