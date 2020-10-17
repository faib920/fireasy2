// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Xml;
#if NETSTANDARD || NETCOREAPP
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Common.Configuration
{
    /// <summary>
    /// 提供配置节的方法。
    /// </summary>
    public interface IConfigurationSection
    {
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        void Initialize(XmlNode section);

#if NETSTANDARD || NETCOREAPP
        void Bind(IConfiguration configuration);
#endif
    }

    public interface IConfigurationSectionWithCount : IConfigurationSection
    {
        int Count { get; }
    }
}
