// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETSTANDARD
using Fireasy.Common.Extensions;
using System;
using System.Configuration;
using System.Xml;

namespace Fireasy.Common.Configuration
{
    /// <summary>
    /// 一个抽象类，提供对配置节的处理。
    /// </summary>
    public abstract class ConfigurationSectionHandler<T> : IConfigurationSectionHandler where T : IConfigurationSection
    {
        /// <summary>
        /// 创建配置节处理程序。
        /// </summary>
        /// <param name="parent">父对象。</param>
        /// <param name="configContext">配置上下文对象。</param>
        /// <param name="section">节 XML 节点。</param>
        /// <returns>创建一个节处理程序的 <see cref="IConfigurationSection"/> 对象。</returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            var obj = typeof(T).New<IConfigurationSection>();
            if (obj != null)
            {
                obj.Initialize(section);
                return obj;
            }

            return null;
        }
    }
}
#endif