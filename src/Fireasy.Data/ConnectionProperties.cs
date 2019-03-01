// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System.Collections.Generic;

namespace Fireasy.Data
{
    /// <summary>
    /// 连接字符串的属性字典。无法继承此类。
    /// </summary>
    public sealed class ConnectionProperties
    {
        private Dictionary<string, string> dictionary = new Dictionary<string, string>();

        /// <summary>
        /// 添加属性。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="value">属性值。</param>
        public void Add(string name, string value)
        {
            dictionary.AddOrReplace(name, value);
        }

        /// <summary>
        /// 获取所有属性名称。
        /// </summary>
        public IEnumerable<string> Names
        {
            get
            {
                return dictionary.Keys;
            }
        }

        /// <summary>
        /// 获取指定名称的属性值。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <returns></returns>
        public string this[string name]
        {
            get
            {
                return dictionary.TryGetValue(name, () => string.Empty);
            }
        }

        /// <summary>
        /// 尝试在一组名称中获取属性值。
        /// </summary>
        /// <param name="names">一个数组，指定可能存在的属性名称。由于连接串可能有多种格式，因此属性名称可能有多个。</param>
        /// <returns></returns>
        public string TryGetValue(params string[] names)
        {
            foreach (var name in names)
            {
                if (dictionary.ContainsKey(name))
                {
                    return dictionary[name];
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 尝试在一组名称中获取属性值。当这一组值都不存在时，返回 <paramref name="defaultValue"/>。
        /// </summary>
        /// <param name="defaultValue">缺省值。</param>
        /// <param name="names">一个数组，指定可能存在的属性名称。由于连接串可能有多种格式，因此属性名称可能有多个。</param>
        /// <returns></returns>
        public string TryGetValueWithDefaultValue(string defaultValue, params string[] names)
        {
            foreach (var name in names)
            {
                if (dictionary.ContainsKey(name))
                {
                    return dictionary[name];
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 尝试设置一组属性的值。
        /// </summary>
        /// <param name="value">设置的属性值。</param>
        /// <param name="names">一个数组，指定可能存在的属性名称。由于连接串可能有多种格式，因此属性名称可能有多个。</param>
        public ConnectionProperties TrySetValue(string value, params string[] names)
        {
            foreach (var name in names)
            {
                if (dictionary.ContainsKey(name))
                {
                    dictionary[name] = value;
                }
            }

            return this;
        }

        /// <summary>
        /// 判断字典中是否包含指定名称的值。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <returns></returns>
        public bool ContainsKey(string name)
        {
            return dictionary.ContainsKey(name);
        }
    }
}
