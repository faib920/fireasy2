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
                        PropertyName = SerializerUtil.GetPropertyName(s)
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
        internal void TrySerialize(object obj, Action serializeMethod)
        {
            if (obj == null)
            {
                serializeMethod();
                return;
            }

            if (objects.IndexOf(obj) != -1)
            {
                throw new SerializationException(SR.GetString(SRKind.LoopReferenceSerialize, obj));
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
        public void Dispose()
        {
            GetAccessors.Clear();
            objects.Clear();
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
    }
}
