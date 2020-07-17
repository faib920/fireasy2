// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 序列化契约解析器。
    /// </summary>
    public interface IContractResolver
    {
        /// <summary>
        /// 获取指定类型的所有属性元数据。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        List<SerializerPropertyMetadata> GetProperties(Type type);

        /// <summary>
        /// 解析属性的名称。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        string ResolvePropertyName(PropertyInfo property);

        /// <summary>
        /// 解析属性名。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        string ResolvePropertyName(string propertyName);
    }

    /// <summary>
    /// 缺省的序列化契约解析器。
    /// </summary>
    public class DefaultContractResolver : IContractResolver
    {
        private readonly SerializeOption _option;
        private readonly ConcurrentDictionary<Type, List<SerializerPropertyMetadata>> _cache = new ConcurrentDictionary<Type, List<SerializerPropertyMetadata>>();

        /// <summary>
        /// 初始化 <see cref="DefaultContractResolver"/> 类的新实例。
        /// </summary>
        /// <param name="option"></param>
        public DefaultContractResolver(SerializeOption option)
        {
            _option = option;
        }

        /// <summary>
        /// 获取指定类型的所有属性元数据。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual List<SerializerPropertyMetadata> GetProperties(Type type)
        {
            return _cache.GetOrAdd(type, k =>
            {
                return k.GetProperties()
                    .Where(s => s.CanRead && !SerializerUtil.IsNoSerializable(_option, s))
                    .Distinct(new SerializerUtil.PropertyEqualityComparer())
                    .Select(s => new SerializerPropertyMetadata
                    {
                        Filter = (p, l) =>
                        {
                            return !SerializerUtil.CheckLazyValueCreate(l, p.Name);
                        },
                        Getter = o => ReflectionCache.GetAccessor(s).GetValue(o),
                        Setter = (o, v) => ReflectionCache.GetAccessor(s).SetValue(o, v),
                        PropertyInfo = s,
                        PropertyName = ResolvePropertyName(s),
                        Formatter = s.GetCustomAttributes<TextFormatterAttribute>().FirstOrDefault()?.Formatter,
                        DefaultValue = s.GetCustomAttributes<DefaultValueAttribute>().FirstOrDefault()?.Value,
                        Converter = s.GetCustomAttributes<TextPropertyConverterAttribute>().FirstOrDefault()?.ConverterType.New<ITextConverter>()
                    })
                    .Where(s => !string.IsNullOrEmpty(s.PropertyName))
                    .ToList();
            });
        }

        /// <summary>
        /// 解析属性的名称。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual string ResolvePropertyName(PropertyInfo property)
        {
            var attr = property.GetCustomAttributes<TextSerializeElementAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.Name;
            }

            return ResolvePropertyName(property.Name);
        }

        /// <summary>
        /// 解析属性名。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual string ResolvePropertyName(string propertyName)
        {
            if (_option.NamingHandling == NamingHandling.Camel && char.IsUpper(propertyName[0]))
            {
                return char.ToLower(propertyName[0]) + propertyName.Substring(1);
            }

            return propertyName;
        }
    }
}
