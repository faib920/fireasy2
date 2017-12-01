// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Fireasy.Common.Configuration;

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 数据库配置实例接口。
    /// </summary>
    public interface IInstanceConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 返回提供者配置名称。
        /// </summary>
        string ProviderName { get; set; }

        /// <summary>
        /// 返回数据提供者类型。
        /// </summary>
        string ProviderType { get; set; }

        /// <summary>
        /// 返回数据库类型。
        /// </summary>
        Type DatabaseType { get; set; }

        /// <summary>
        /// 返回数据库连接字符串。
        /// </summary>
        string ConnectionString { get; set; }
    }

    /// <summary>
    /// 缺省的数据库配置实例。
    /// </summary>
    public class DefaultInstanceConfigurationSetting : IInstanceConfigurationSetting
    {
        /// <summary>
        /// 返回提供者配置名称。
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// 返回数据提供者类型。
        /// </summary>
        public string ProviderType { get; set; }

        /// <summary>
        /// 返回数据库类型。
        /// </summary>
        public Type DatabaseType { get; set; }

        /// <summary>
        /// 返回数据库连接字符串。
        /// </summary>
        public string ConnectionString { get; set; }
    }

}
