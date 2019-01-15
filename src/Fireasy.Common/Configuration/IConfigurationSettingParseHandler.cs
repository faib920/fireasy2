// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Xml;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Common.Configuration
{
    /// <summary>
    /// 提供对配置项的解析方法。
    /// </summary>
    public interface IConfigurationSettingParseHandler
    {
        /// <summary>
        /// 将节点信息解析为配置项。
        /// </summary>
        /// <param name="section">配置节点。</param>
        /// <returns></returns>
        IConfigurationSettingItem Parse(XmlNode section);

#if NETSTANDARD
        /// <summary>
        /// 将节点信息解析为配置项。
        /// </summary>
        /// <param name="configuration">配置属性。</param>
        /// <returns></returns>
        IConfigurationSettingItem Parse(IConfiguration configuration);
#endif
    }
}
