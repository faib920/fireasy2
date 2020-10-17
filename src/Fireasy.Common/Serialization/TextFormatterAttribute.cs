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
    /// 为 DateTime、Decimal 等类型指定文本格式化的格式。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TextFormatterAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="TextFormatterAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="formatter">文本格式。</param>
        public TextFormatterAttribute(string formatter)
        {
            Formatter = formatter;
        }

        /// <summary>
        /// 获取或设置文本格式。
        /// </summary>
        public string Formatter { get; set; }
    }
}
