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
    /// 表示反序列化时代替的缺省值。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultValueAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="DefaultValueAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="value">缺省值。</param>
        public DefaultValueAttribute(object value)
        {
            Value = value;
        }

        /// <summary>
        /// 获取或设置缺省值。
        /// </summary>
        public object Value { get; set; }
    }
}
