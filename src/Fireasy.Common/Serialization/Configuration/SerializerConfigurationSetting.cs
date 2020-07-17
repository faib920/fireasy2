// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Configuration;
using System;

namespace Fireasy.Common.Serialization.Configuration
{
    /// <summary>
    /// 文本序列化器配置信息。
    /// </summary>
    public class SerializerConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置配置的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置文本序列化器的类型。
        /// </summary>
        public Type SerializerType { get; set; }
    }
}
