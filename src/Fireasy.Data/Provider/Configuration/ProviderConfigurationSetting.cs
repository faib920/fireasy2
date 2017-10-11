// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;

namespace Fireasy.Data.Provider.Configuration
{
    /// <summary>
    /// 提供者的配置。
    /// </summary>
    public class ProviderConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 初始化 <see cref="ProviderConfigurationSetting"/> 类的新实例。
        /// </summary>
        /// <param name="services"></param>
        internal ProviderConfigurationSetting(IEnumerable<Type> services)
        {
            ServiceTypes = services.ToReadOnly();
        }

        /// <summary>
        /// 获取或设置配置的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置提供者的类型。
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 获取提供服务的类型列表。
        /// </summary>
        public ReadOnlyCollection<Type> ServiceTypes { get; private set; }
    }
}
