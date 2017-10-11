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
    /// 为应用程序提供配置信息的宿主支持。
    /// </summary>
    public interface IConfigurationSettingHostService
    {
        /// <summary>
        /// 将配置信息附加给应用程序。
        /// </summary>
        /// <param name="setting">配置项。</param>
        void Attach(IConfigurationSettingItem setting);

        /// <summary>
        /// 从应用程序中获取本应的配置信息。
        /// </summary>
        /// <returns></returns>
        IConfigurationSettingItem GetSetting();
    }
}
