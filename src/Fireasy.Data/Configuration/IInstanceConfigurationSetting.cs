// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Configuration;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// 返回集群连接串配置。
        /// </summary>
        List<ClusteredConnectionSetting> Clusters { get; set; }
    }

    /// <summary>
    /// 缺省的数据库配置实例。
    /// </summary>
    public class DefaultInstanceConfigurationSetting : IInstanceConfigurationSetting
    {
        /// <summary>
        /// 返回提供者配置名称。
        /// </summary>
        public virtual string ProviderName { get; set; }

        /// <summary>
        /// 返回数据提供者类型。
        /// </summary>
        public virtual string ProviderType { get; set; }

        /// <summary>
        /// 返回数据库类型。
        /// </summary>
        public virtual Type DatabaseType { get; set; }

        /// <summary>
        /// 返回数据库连接字符串。
        /// </summary>
        public virtual string ConnectionString { get; set; }

        /// <summary>
        /// 返回集群配置串。
        /// </summary>
        public virtual List<ClusteredConnectionSetting> Clusters { get; set; } = new List<ClusteredConnectionSetting>();
    }

    /// <summary>
    /// 连接的集群。
    /// </summary>
    public class ClusteredConnectionSetting
    {
        /// <summary>
        /// 获取或设置分布式模式。
        /// </summary>
        public DistributedMode Mode { get; set; }

        /// <summary>
        /// 获取或设置权重。
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// 返回数据库连接字符串。
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
