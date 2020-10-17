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
    /// 标识应用程序采用何种配置。无法继承此类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConfigurationSettingAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="ConfigurationSettingAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="type">配置项的类型。</param>
        public ConfigurationSettingAttribute(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// 获取或设置配置项的类型。
        /// </summary>
        public Type Type { get; set; }
    }
}
