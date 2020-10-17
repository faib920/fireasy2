// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 提供序列化时访问类型内部元素的一组方法。无法继承此类。
    /// </summary>
    public sealed class TextSerializeElementPropertyHelper
    {
        private readonly Dictionary<string, PropertyDescriptor> _properties;

        /// <summary>
        /// 初始化 <see cref="TextSerializeElementPropertyHelper"/> 类的新实例。
        /// </summary>
        /// <param name="type">被序列化的对象的类型。</param>
        /// <param name="camel">元素名称是否使用 Camel 命名规则。</param>
        public TextSerializeElementPropertyHelper(Type type, bool camel)
        {
            _properties = new Dictionary<string, PropertyDescriptor>();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(type))
            {
                var ele = property.GetCustomAttributes<TextSerializeElementAttribute>().FirstOrDefault();

                //如果使用Camel命名，则名称第一位小写
                var name = camel ? char.ToLower(property.Name[0]) + property.Name.Substring(1) : property.Name;

                //如果使用了 TextSerializeElementAttribute 修饰，则取该特性指定的名称
                _properties.Add(ele == null ? name : ele.Name, property);
            }
        }

        /// <summary>
        /// 获取指定名称的属性。
        /// </summary>
        /// <param name="name">用于标识属性名称的文本。</param>
        /// <returns>与名称对应的属性。</returns>
        public PropertyDescriptor GetProperty(string name)
        {
            if (_properties.TryGetValue(name, out PropertyDescriptor result))
            {
                return result;
            }

            return null;
        }
    }
}
