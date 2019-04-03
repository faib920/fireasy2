// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 序列化上下文对象。
    /// </summary>
    public class SerializeContext : Scope<SerializeContext>
    {
        private readonly List<object> objects = new List<object>();

        /// <summary>
        /// 初始化 <see cref="SerializeContext"/> 类的新实例。
        /// </summary>
        public SerializeContext()
        {
            GetAccessors = new Dictionary<Type, List<PropertyGetAccessorCache>>();
            SetAccessors = new Dictionary<Type, Dictionary<string, PropertyAccessor>>();
        }

        /// <summary>
        /// 获取或设置读取的类属性缓存。
        /// </summary>
        public Dictionary<Type, List<PropertyGetAccessorCache>> GetAccessors { get; set; }

        /// <summary>
        /// 获取或设置写入的类属性缓存。
        /// </summary>
        public Dictionary<Type, Dictionary<string, PropertyAccessor>> SetAccessors { get; set; }

        /// <summary>
        /// 获取或设置 <see cref="SerializeOption"/>。
        /// </summary>
        public SerializeOption Option { get; set; }

        /// <summary>
        /// 获取当前的 <see cref="PropertySerialzeInfo"/> 对象。
        /// </summary>
        public PropertySerialzeInfo SeriaizeInfo { get; internal set; }

        /// <summary>
        /// 获取指定类型的属性访问缓存。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<PropertyGetAccessorCache> GetAccessorCache(Type type)
        {
            return GetAccessors.TryGetValue(type, () =>
            {
                return type.GetProperties()
                    .Where(s => s.CanRead && !SerializerUtil.IsNoSerializable(Option, s))
                    .Distinct(new SerializerUtil.PropertyEqualityComparer())
                    .Select(s => new PropertyGetAccessorCache
                    {
                        Accessor = ReflectionCache.GetAccessor(s),
                        Filter = (p, l) =>
                        {
                            return !SerializerUtil.CheckLazyValueCreate(l, p.Name);
                        },
                        PropertyInfo = s,
                        PropertyName = SerializerUtil.GetPropertyName(s),
                        Formatter = s.GetCustomAttributes<TextFormatterAttribute>().FirstOrDefault()?.Formatter,
                        DefaultValue = s.GetCustomAttributes<DefaultValueAttribute>().FirstOrDefault()?.Value,
                        Converter = s.GetCustomAttributes<TextPropertyConverterAttribute>().FirstOrDefault()?.ConverterType.New<ITextConverter>()
                    })
                    .Where(s => !string.IsNullOrEmpty(s.PropertyName))
                    .ToList();
            });
        }

        /// <summary>
        /// 尝试使用方法序列化对象。
        /// </summary>
        /// <param name="obj">要锁定的对象。</param>
        /// <param name="serializeMethod">被锁定的方法。</param>
        /// <exception cref="SerializationException">该对象被循环引用，即嵌套引用。</exception>
        public void TrySerialize(object obj, Action serializeMethod)
        {
            if (obj == null)
            {
                serializeMethod();
                return;
            }

            if (objects.IndexOf(obj) != -1)
            {
                if (Option.ReferenceLoopHandling == ReferenceLoopHandling.Error)
                {
                    throw new SerializationException(SR.GetString(SRKind.LoopReferenceSerialize, obj));
                }
                else if (Option.ReferenceLoopHandling == ReferenceLoopHandling.Ignore)
                {
                    return;
                }
            }

            try
            {
                objects.Add(obj);
                serializeMethod();
            }
            finally
            {
                objects.Remove(obj);
            }
        }

        /// <summary>
        /// 释放对象所占用的所有资源。
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            GetAccessors.Clear();
            objects.Clear();
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// 属性读取器的缓存。
    /// </summary>
    public class PropertyGetAccessorCache
    {
        /// <summary>
        /// 获取属性对应的访问器。
        /// </summary>
        public PropertyAccessor Accessor { get; internal set; }

        /// <summary>
        /// 获取属性过滤的一个方法。
        /// </summary>
        public Func<PropertyInfo, ILazyManager, bool> Filter { get; internal set; }

        /// <summary>
        /// 获取被缓存的 <see cref="PropertyInfo"/> 对象。
        /// </summary>
        public PropertyInfo PropertyInfo { get; internal set; }

        /// <summary>
        /// 获取被缓存的属性名称。
        /// </summary>
        public string PropertyName { get; internal set; }

        /// <summary>
        /// 获取格式化文本的格式。
        /// </summary>
        public string Formatter { get; internal set; }

        /// <summary>
        /// 获取缺省的值。
        /// </summary>
        public object DefaultValue { get; internal set; }

        /// <summary>
        /// 获取属性上的转换器。
        /// </summary>
        public ITextConverter Converter { get; internal set; }
    }

    /// <summary>
    /// 属性的序列化信息。
    /// </summary>
    public class PropertySerialzeInfo
    {
        public PropertySerialzeInfo(PropertyGetAccessorCache cache)
        {
            ObjectType = ObjectType.GeneralObject;
            PropertyType = cache.PropertyInfo.PropertyType;
            PropertyName = cache.PropertyName;
            Formatter = cache.Formatter;
        }

        public PropertySerialzeInfo(ObjectType objectType, Type propertyType, string propertyName)
        {
            ObjectType = objectType;
            PropertyType = propertyType;
            PropertyName = propertyName;
        }

        /// <summary>
        /// 获取对象的类型。
        /// </summary>
        public ObjectType ObjectType { get; private set; }

        /// <summary>
        /// 获取属性名称。
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// 获取属性的类型。
        /// </summary>
        public Type PropertyType { get; private set; }

        /// <summary>
        /// 获取文本格式化。
        /// </summary>
        public string Formatter { get; private set; }
    }

    /// <summary>
    /// 对象类型。
    /// </summary>
    public enum ObjectType
    {
        /// <summary>
        /// 一般的对象。
        /// </summary>
        GeneralObject,
        /// <summary>
        /// 动态对象。
        /// </summary>
        DynamicObject,
        /// <summary>
        /// 字典。
        /// </summary>
        Dictionary,
    }
}
