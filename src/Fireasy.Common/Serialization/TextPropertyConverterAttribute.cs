// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 为属性指定一个转换器。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TextPropertyConverterAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="TextPropertyConverterAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="converterType">一个转换器类型。</param>
        public TextPropertyConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }

        /// <summary>
        /// 获取或设置转换器类型。
        /// </summary>
        public Type ConverterType { get; set; }
    }
}
