// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Fireasy.Common.Configuration;

namespace Fireasy.Common.Threading.Configuration
{
    /// <summary>
    /// 锁的配置信息。
    /// </summary>
    public class LockerConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置配置的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置锁的类型。
        /// </summary>
        public Type LockerType { get; set; }
    }
}
