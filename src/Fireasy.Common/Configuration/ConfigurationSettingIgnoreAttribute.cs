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
    /// 表示忽略配置节解析的处理程序。无法继承此类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConfigurationSettingIgnoreAttribute : Attribute
    {
    }
}
