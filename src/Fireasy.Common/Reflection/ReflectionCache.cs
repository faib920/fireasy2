// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Fireasy.Common.Reflection
{
    /// <summary>
    /// 反射的缓存管理器。
    /// </summary>
    public static class ReflectionCache
    {
        private static ConcurrentDictionary<FieldInfo, FieldAccessor> fieldAccessors = new ConcurrentDictionary<FieldInfo, FieldAccessor>();
        private static ConcurrentDictionary<PropertyInfo, PropertyAccessor> propertyAccessors = new ConcurrentDictionary<PropertyInfo, PropertyAccessor>();
        private static ConcurrentDictionary<MethodInfo, MethodInvoker> methodInvoker = new ConcurrentDictionary<MethodInfo, MethodInvoker>();
        private static ConcurrentDictionary<ConstructorInfo, ConstructorInvoker> construtorInvoker = new ConcurrentDictionary<ConstructorInfo, ConstructorInvoker>();

        /// <summary>
        /// 获取字段的访问器。
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static FieldAccessor GetAccessor(FieldInfo field)
        {
            var lazy = new Lazy<FieldAccessor>(() => new FieldAccessor(field));
            return fieldAccessors.GetOrAdd(field, s => lazy.Value);
        }

        /// <summary>
        /// 获取属性的访问器。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static PropertyAccessor GetAccessor(PropertyInfo property)
        {
            var lazy = new Lazy<PropertyAccessor>(() => new PropertyAccessor(property));
            return propertyAccessors.GetOrAdd(property, s => lazy.Value);
        }

        /// <summary>
        /// 获取方法的执行器。
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MethodInvoker GetInvoker(MethodInfo method)
        {
            var lazy = new Lazy<MethodInvoker>(() => new MethodInvoker(method));
            return methodInvoker.GetOrAdd(method, s => lazy.Value);
        }

        /// <summary>
        /// 获取构造函数的执行器。
        /// </summary>
        /// <param name="constructor"></param>
        /// <returns></returns>
        public static ConstructorInvoker GetInvoker(ConstructorInfo constructor)
        {
            var lazy = new Lazy<ConstructorInvoker>(() => new ConstructorInvoker(constructor));
            return construtorInvoker.GetOrAdd(constructor, s => lazy.Value);
        }
    }
}
