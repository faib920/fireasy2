// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 序列化上下文对象。
    /// </summary>
    public class SerializeContext : Scope<SerializeContext>
    {
        private readonly List<object> _objects = new List<object>();
        private static readonly ConcurrentDictionary<Type, List<SerializerPropertyMetadata>> _cache = new ConcurrentDictionary<Type, List<SerializerPropertyMetadata>>();

        /// <summary>
        /// 获取或设置 <see cref="SerializeOption"/>。
        /// </summary>
        public SerializeOption Option { get; set; }

        /// <summary>
        /// 获取当前的 <see cref="PropertySerialzeInfo"/> 对象。
        /// </summary>
        public PropertySerialzeInfo SerializeInfo { get; internal set; }

        /// <summary>
        /// 获取指定类型的属性元数据。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valueCreator"></param>
        /// <returns></returns>
        public List<SerializerPropertyMetadata> GetProperties(Type type, Func<List<SerializerPropertyMetadata>> valueCreator)
        {
            return _cache.GetOrAdd(type, k => valueCreator());
        }

        /// <summary>
        /// 尝试使用方法序列化对象。
        /// </summary>
        /// <param name="obj">要锁定的对象。</param>
        /// <param name="serializeMethod">被锁定的方法。</param>
        /// <exception cref="SerializationException">该对象被循环引用，即嵌套引用。</exception>
        public void TrySerialize(object obj, Action<object> serializeMethod)
        {
            if (obj == null || Option.ReferenceLoopHandling == ReferenceLoopHandling.None)
            {
                serializeMethod(obj);
                return;
            }

            if (_objects.IndexOf(obj) != -1)
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
                _objects.Add(obj);
                serializeMethod(obj);
            }
            finally
            {
                _objects.Remove(obj);
            }
        }

        /// <summary>
        /// 释放对象所占用的所有资源。
        /// </summary>
        /// <param name="disposing"></param>
        protected override bool Dispose(bool disposing)
        {
            _objects.Clear();

            return base.Dispose(disposing);
        }
    }

    /// <summary>
    /// 序列化属性映射元数据。
    /// </summary>
    public class SerializerPropertyMetadata
    {
        /// <summary>
        /// 获取或设置属性的取值方法。
        /// </summary>
        public Func<object, object> Getter { get; set; }

        /// <summary>
        /// 获取或设置属性的赋值方法。
        /// </summary>
        public Action<object, object> Setter { get; set; }

        /// <summary>
        /// 获取或设置属性过滤的一个方法。
        /// </summary>
        public Func<PropertyInfo, ILazyManager, bool> Filter { get; internal set; }

        /// <summary>
        /// 获取或设置被缓存的 <see cref="PropertyInfo"/> 对象。
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// 获取或设置被缓存的属性名称。
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 获取或设置格式化文本的格式。
        /// </summary>
        public string Formatter { get; set; }

        /// <summary>
        /// 获取或设置缺省的值。
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// 获取或设置属性上的转换器。
        /// </summary>
        public ITextConverter Converter { get; set; }
    }

    /// <summary>
    /// 属性的序列化信息。
    /// </summary>
    public class PropertySerialzeInfo
    {
        public PropertySerialzeInfo(SerializerPropertyMetadata metadata)
        {
            ObjectType = ObjectType.GeneralObject;
            PropertyInfo = metadata.PropertyInfo;
            PropertyType = metadata.PropertyInfo.PropertyType;
            PropertyName = metadata.PropertyName;
            Formatter = metadata.Formatter;
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
        /// 获取属性元数据。
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

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
