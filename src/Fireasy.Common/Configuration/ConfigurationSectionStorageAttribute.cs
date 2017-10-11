// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Configuration
{
    /// <summary>
    /// 标识配置所存储的节点路径。无法继承此类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConfigurationSectionStorageAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="ConfigurationSectionStorageAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="name"></param>
        public ConfigurationSectionStorageAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 获取或设置存储节点的名称。
        /// </summary>
        public string Name { get; set; }
    }
}
