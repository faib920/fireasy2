// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Configuration;
using System.Collections.ObjectModel;

namespace Fireasy.Common.Ioc.Configuration
{
    /// <summary>
    /// 表示容器的配置信息。无法继承此类。
    /// </summary>
    public sealed class ContainerConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置配置的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取服务与组件的注册键对。
        /// </summary>
        public ReadOnlyCollection<RegistrationSetting> Registrations { get; set; }
    }
}
