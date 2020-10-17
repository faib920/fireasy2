// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Fireasy.Common.Reflection
{
    /// <summary>
    /// 反射的缓存管理器。
    /// </summary>
    public static class ReflectionCache
    {
        private static readonly ConcurrentDictionary<FieldInfo, FieldAccessor> _fieldAccessors = new ConcurrentDictionary<FieldInfo, FieldAccessor>();
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<PropertyInfo, IPropertyAccessor>> _propertyAccessors = new ConcurrentDictionary<Type, ConcurrentDictionary<PropertyInfo, IPropertyAccessor>>();
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<MethodInfo, MethodInvoker>> _methodInvokers = new ConcurrentDictionary<Type, ConcurrentDictionary<MethodInfo, MethodInvoker>>();
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<ConstructorInfo, ConstructorInvoker>> _construtorInvokers = new ConcurrentDictionary<Type, ConcurrentDictionary<ConstructorInfo, ConstructorInvoker>>();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<MemberInfo, MemberInfo>> _memberCache = new ConcurrentDictionary<string, ConcurrentDictionary<MemberInfo, MemberInfo>>();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<MemberInfo[], MemberInfo>> _memberArrayCache = new ConcurrentDictionary<string, ConcurrentDictionary<MemberInfo[], MemberInfo>>();

        /// <summary>
        /// 获取字段的访问器。
        /// </summary>
        /// <param name="field">字段。</param>
        /// <returns></returns>
        public static FieldAccessor GetAccessor(FieldInfo field)
        {
            return _fieldAccessors.GetOrAdd(field, key => new FieldAccessor(key));
        }

        /// <summary>
        /// 获取属性的访问器。
        /// </summary>
        /// <param name="property">属性。</param>
        /// <returns></returns>
        public static PropertyAccessor GetAccessor(PropertyInfo property)
        {
            var dict = _propertyAccessors.GetOrAdd(property.DeclaringType, k => new ConcurrentDictionary<PropertyInfo, IPropertyAccessor>());
            return (PropertyAccessor)dict.GetOrAdd(property, key => new PropertyAccessor(key));
        }

        public static PropertyAccessor<T> GetAccessor<T>(PropertyInfo property)
        {
            var dict = _propertyAccessors.GetOrAdd(property.DeclaringType, k => new ConcurrentDictionary<PropertyInfo, IPropertyAccessor>());
            return (PropertyAccessor<T>)dict.GetOrAdd(property, key => new PropertyAccessor<T>(key));
        }

        /// <summary>
        /// 获取方法的执行器。
        /// </summary>
        /// <param name="method">方法。</param>
        /// <returns></returns>
        public static MethodInvoker GetInvoker(MethodInfo method)
        {
            var dict = _methodInvokers.GetOrAdd(method.DeclaringType, k => new ConcurrentDictionary<MethodInfo, MethodInvoker>());
            return dict.GetOrAdd(method, key => new MethodInvoker(key));
        }

        /// <summary>
        /// 获取构造函数的执行器。
        /// </summary>
        /// <param name="constructor">构造函数。</param>
        /// <returns></returns>
        public static ConstructorInvoker GetInvoker(ConstructorInfo constructor)
        {
            var dict = _construtorInvokers.GetOrAdd(constructor.DeclaringType, k => new ConcurrentDictionary<ConstructorInfo, ConstructorInvoker>());
            return dict.GetOrAdd(constructor, key => new ConstructorInvoker(key));
        }

        /// <summary>
        /// 获取对应的成员缓存。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="key">键名。</param>
        /// <param name="source">源成员。</param>
        /// <param name="creator">创建新成员的函数。</param>
        /// <returns></returns>
        public static TTarget GetMember<TSource, TTarget>(string key, TSource source, Func<TSource, TTarget> creator) where TSource : MemberInfo where TTarget : MemberInfo
        {
            var dict = _memberCache.GetOrAdd(key, k => new ConcurrentDictionary<MemberInfo, MemberInfo>());
            return (TTarget)dict.GetOrAdd(source, k => creator(source));
        }

        /// <summary>
        /// 获取对应的成员缓存。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="key">键名。</param>
        /// <param name="source">源成员。</param>
        /// <param name="arg1">额外的参数1。</param>
        /// <param name="creator">创建新成员的函数。</param>
        /// <returns></returns>
        public static TTarget GetMember<TSource, TArg1, TTarget>(string key, TSource source, TArg1 arg1, Func<TSource, TArg1, TTarget> creator) where TSource : MemberInfo where TTarget : MemberInfo
        {
            var dict = _memberCache.GetOrAdd(key, k => new ConcurrentDictionary<MemberInfo, MemberInfo>());
            return (TTarget)dict.GetOrAdd(source, k => creator(source, arg1));
        }

        /// <summary>
        /// 获取对应的成员缓存。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="key">键名。</param>
        /// <param name="source">源成员。</param>
        /// <param name="arg1">额外的参数1。</param>
        /// <param name="arg2">额外的参数2。</param>
        /// <param name="creator">创建新成员的函数。</param>
        /// <returns></returns>
        public static TTarget GetMember<TSource, TArg1, TArg2, TTarget>(string key, TSource source, TArg1 arg1, TArg2 arg2, Func<TSource, TArg1, TArg2, TTarget> creator) where TSource : MemberInfo where TTarget : MemberInfo
        {
            var dict = _memberCache.GetOrAdd(key, k => new ConcurrentDictionary<MemberInfo, MemberInfo>());
            return (TTarget)dict.GetOrAdd(source, k => creator(source, arg1, arg2));
        }

        /// <summary>
        /// 获取对应的成员缓存。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="key">键名。</param>
        /// <param name="source">源成员。</param>
        /// <param name="arg1">额外的参数1。</param>
        /// <param name="arg2">额外的参数2。</param>
        /// <param name="arg3">额外的参数3。</param>
        /// <param name="creator">创建新成员的函数。</param>
        /// <returns></returns>
        public static TTarget GetMember<TSource, TArg1, TArg2, TArg3, TTarget>(string key, TSource source, TArg1 arg1, TArg2 arg2, TArg3 arg3, Func<TSource, TArg1, TArg2, TArg3, TTarget> creator) where TSource : MemberInfo where TTarget : MemberInfo
        {
            var dict = _memberCache.GetOrAdd(key, k => new ConcurrentDictionary<MemberInfo, MemberInfo>());
            return (TTarget)dict.GetOrAdd(source, k => creator(source, arg1, arg2, arg3));
        }

        /// <summary>
        /// 获取对应的成员缓存。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="key">键名。</param>
        /// <param name="source">源成员。</param>
        /// <param name="creator">创建新成员的函数。</param>
        /// <returns></returns>
        public static TTarget GetMember<TSource, TTarget>(string key, TSource[] source, Func<TSource[], TTarget> creator) where TSource : MemberInfo where TTarget : MemberInfo
        {
            var dict = _memberArrayCache.GetOrAdd(key, k => new ConcurrentDictionary<MemberInfo[], MemberInfo>(new MemberKeyComparer()));
            return (TTarget)dict.GetOrAdd(source, k => creator(source));
        }

        private class MemberKeyComparer : IEqualityComparer<MemberInfo[]>
        {
            bool IEqualityComparer<MemberInfo[]>.Equals(MemberInfo[] x, MemberInfo[] y)
            {
                if (x.Length != y.Length)
                {
                    return false;
                }

                for (int i = 0, n = x.Length; i < n; i++)
                {
                    if (x[i] != y[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            int IEqualityComparer<MemberInfo[]>.GetHashCode(MemberInfo[] obj)
            {
                if (obj.Length == 0)
                {
                    return 0;
                }

                var hash = obj[0].GetHashCode();
                if (obj.Length == 1)
                {
                    return hash;
                }

                for (int i = 1, n = obj.Length; i < n; i++)
                {
                    hash ^= obj[i].GetHashCode();
                }

                return hash;
            }
        }
    }
}
