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
    /// 表示配置节所使用的解析程序类型。无法继承此类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConfigurationSettingParseTypeAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="ConfigurationSettingParseTypeAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="type">解析类的类型。</param>
        public ConfigurationSettingParseTypeAttribute(Type type)
        {
            HandlerType = type;
        }

        /// <summary>
        /// 获取或设置解析类的类型。
        /// </summary>
        public Type HandlerType { get; set; }
    }
}
