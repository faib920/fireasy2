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
    /// 配置项。
    /// </summary>
    public interface IConfigurationSettingItem
    {
    }

    /// <summary>
    /// 表示命名的配置项。
    /// </summary>
    public interface INamedIConfigurationSettingItem : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置名称。
        /// </summary>
        string Name { get; set; }
    }
}
