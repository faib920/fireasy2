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
    /// 为构造函数提供参数的绑定，以使可以通过构造函数反序列化实例。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class TextSerializeParameterBindAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="TextSerializeParameterBindAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="name"></param>
        public TextSerializeParameterBindAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 获取或设置绑定的名称。
        /// </summary>
        public string Name { get; set; }
    }
}
