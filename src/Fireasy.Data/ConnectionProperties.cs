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
        private readonly Dictionary<string, PropertyValue> properties = new Dictionary<string, PropertyValue>();
        private readonly ConnectionString connectionString;

        /// <summary>
        /// 属性值。
        /// </summary>
        private sealed class PropertyValue
        {
            public PropertyValue(bool isCustomized, string value)
            {
                IsCustomized = isCustomized;
                Value = value;
            }

            /// <summary>
            /// 获取或设置是否自定的属性。
            /// </summary>
            public bool IsCustomized { get; set; }

            /// <summary>
            /// 获取或设置属性的值。
            /// </summary>
            public string Value { get; set; }

            public override string ToString()
            {
                return Value;
            }
        }

        public ConnectionProperties(ConnectionString connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// 添加属性。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="value">属性值。</param>
        /// <param name="isCustomized">是否自定的属性。</param>
        public void Add(string name, string value, bool isCustomized = false)
        {
            properties.AddOrReplace(name, new PropertyValue(isCustomized, value));
        }

        /// <summary>
        /// 获取所有属性名称。
        /// </summary>
        public IEnumerable<string> Names
        {
            get
            {
                return properties.Keys;
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
                if (properties.TryGetValue(name, out PropertyValue value))
                {
                    return value.Value;
                }

                return string.Empty;
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
                if (properties.ContainsKey(name))
                {
                    return properties[name].Value;
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
                if (properties.ContainsKey(name))
                {
                    return properties[name].Value;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 尝试设置属性的值。
        /// </summary>
        /// <param name="value">设置的属性值。</param>
        /// <param name="name">属性名称。</param>
        public ConnectionProperties TrySetValue(string value, string name)
        {
            if (properties.ContainsKey(name))
            {
                properties[name].Value = value;
            }

            return this;
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
                if (properties.ContainsKey(name))
                {
                    properties[name].Value = value;
                }
            }

            return this;
        }

        /// <summary>
        /// 返回属性是否为自定的。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsCustomized(string name)
        {
            if (properties.TryGetValue(name, out PropertyValue value))
            {
                return value.IsCustomized;
            }

            return false;
        }

        /// <summary>
        /// 更新所有属性到 <see cref="ConnectionString"/> 实例中。
        /// </summary>
        public void Update()
        {
            connectionString.Update();
        }

        /// <summary>
        /// 判断字典中是否包含指定名称的值。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <returns></returns>
        public bool ContainsKey(string name)
        {
            return properties.ContainsKey(name);
        }
    }
}
