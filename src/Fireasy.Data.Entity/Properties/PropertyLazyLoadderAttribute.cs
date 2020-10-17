// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity.Properties
{
    /// <summary>
    /// 定义属性的延迟加载的 <see cref="IPropertyLazyLoadder"/> 接口。
    /// </summary>
    public sealed class PropertyLazyLoadderAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="PropertyLazyLoadderAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="loadderType"></param>
        public PropertyLazyLoadderAttribute(Type loadderType)
        {
            LoadderType = loadderType;
        }

        /// <summary>
        /// 获取或设置实现延迟加载的类型。
        /// </summary>
        public Type LoadderType { get; set; }
    }
}
