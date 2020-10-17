// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETCOREAPP
using Fireasy.Common.Configuration;
using System.Collections.Generic;
using System.Web.Optimization;

namespace Fireasy.Web.Mvc.Configuration
{
    /// <summary>
    /// 将多个脚本或样式文件打包形成一个 <see cref="Bundle"/> 组。
    /// </summary>
    public class BundleGroupConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 初始化 <see cref="BundleGroupConfigurationSetting"/> 类的新实例。
        /// </summary>
        public BundleGroupConfigurationSetting()
        {
            Resources = new List<ResourcePath>();
        }

        /// <summary>
        /// 获取或设置资源文件的列表。
        /// </summary>
        public List<ResourcePath> Resources { get; set; }
    }

    /// <summary>
    /// 资源路径。
    /// </summary>
    public class ResourcePath
    {
        /// <summary>
        /// 获取或设置路径。
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 获取或设置类型。
        /// </summary>
        public ResourceType Type { get; set; }
    }
}
#endif