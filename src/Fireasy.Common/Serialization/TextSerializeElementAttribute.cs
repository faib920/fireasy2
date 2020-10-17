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
    /// 表示此属性序列化后的元素名称。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class TextSerializeElementAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="TextSerializeElementAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="name">用于标识元素的名称。</param>
        public TextSerializeElementAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 获取或设置元素名称。
        /// </summary>
        public string Name { get; set; }
    }
}
