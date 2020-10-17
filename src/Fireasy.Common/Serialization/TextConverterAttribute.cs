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
    /// 为类型指定一个转换器。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TextConverterAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="TextConverterAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="converterType">一个转换器类型。</param>
        public TextConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }

        /// <summary>
        /// 获取或设置转换器类型。
        /// </summary>
        public Type ConverterType { get; set; }
    }
}
