// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System.Reflection;

namespace Fireasy.Common.Reflection
{
    /// <summary>
    /// 反射的缓存管理器。
    /// </summary>
    public static class ReflectionCache
    {
        private static SafetyDictionary<FieldInfo, FieldAccessor> fieldAccessors = new SafetyDictionary<FieldInfo, FieldAccessor>();
        private static SafetyDictionary<PropertyInfo, PropertyAccessor> propertyAccessors = new SafetyDictionary<PropertyInfo, PropertyAccessor>();
        private static SafetyDictionary<MethodInfo, MethodInvoker> methodInvoker = new SafetyDictionary<MethodInfo, MethodInvoker>();
        private static SafetyDictionary<ConstructorInfo, ConstructorInvoker> construtorInvoker = new SafetyDictionary<ConstructorInfo, ConstructorInvoker>();

        /// <summary>
        /// 获取字段的访问器。
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static FieldAccessor GetAccessor(FieldInfo field)
        {
            return fieldAccessors.GetOrAdd(field, () => new FieldAccessor(field));
        }

        /// <summary>
        /// 获取属性的访问器。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static PropertyAccessor GetAccessor(PropertyInfo property)
        {
            return propertyAccessors.GetOrAdd(property, () => new PropertyAccessor(property));
        }

        /// <summary>
        /// 获取方法的执行器。
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MethodInvoker GetInvoker(MethodInfo method)
        {
            return methodInvoker.GetOrAdd(method, () => new MethodInvoker(method));
        }

        /// <summary>
        /// 获取构造函数的执行器。
        /// </summary>
        /// <param name="constructor"></param>
        /// <returns></returns>
        public static ConstructorInvoker GetInvoker(ConstructorInfo constructor)
        {
            return construtorInvoker.GetOrAdd(constructor, () => new ConstructorInvoker(constructor));
        }
    }
}
