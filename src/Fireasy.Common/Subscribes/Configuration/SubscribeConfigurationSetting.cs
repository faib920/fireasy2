// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Configuration;
using System;

namespace Fireasy.Common.Subscribes.Configuration
{
    /// <summary>
    /// 缓存管理配置信息。
    /// </summary>
    public class SubscribeConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置配置的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置缓存管理器的类型。
        /// </summary>
        public Type SubscriberType { get; set; }
    }
}
