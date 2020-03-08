// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Reflection;
using Fireasy.Common.Extensions;
using System.Linq;
using System;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// LINQ解析的实用功能。
    /// </summary>
    public static class TranslateUtils
    {
        private readonly static ConcurrentDictionary<MethodInfo, IMethodCallBinder> defBinders = new ConcurrentDictionary<MethodInfo, IMethodCallBinder>();
        private readonly static ConcurrentDictionary<Func<MethodInfo, bool>, IMethodCallBinder> matchBinders = new ConcurrentDictionary<Func<MethodInfo, bool>, IMethodCallBinder>();
        private readonly static ConcurrentDictionary<MethodInfo, string> functions = new ConcurrentDictionary<MethodInfo, string>();

        /// <summary>
        /// 添加方法调用的绑定。
        /// </summary>
        /// <param name="method">要绑定的方法。</param>
        /// <param name="binder"></param>
        public static void AddMethodBinder(MethodInfo method, IMethodCallBinder binder)
        {
            defBinders.TryAdd(method, binder);
        }

        /// <summary>
        /// 添加方法调用的绑定。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method">要绑定的方法。</param>
        public static void AddMethodBinder<T>(MethodInfo method) where T : IMethodCallBinder
        {
            AddMethodBinder(method, typeof(T).New<IMethodCallBinder>());
        }

        /// <summary>
        /// 添加方法调用的绑定。
        /// </summary>
        /// <param name="matcher">方法匹配器。</param>
        /// <param name="binder"></param>
        public static void AddMethodBinder(Func<MethodInfo, bool> matcher, IMethodCallBinder binder)
        {
            matchBinders.TryAdd(matcher, binder);
        }

        /// <summary>
        /// 添加方法调用的绑定。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matcher">方法匹配器。</param>
        public static void AddMethodBinder<T>(Func<MethodInfo, bool> matcher) where T : IMethodCallBinder
        {
            AddMethodBinder(matcher, typeof(T).New<IMethodCallBinder>());
        }

        /// <summary>
        /// 获取自定义的函数。
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string GetCustomFunction(MethodInfo method)
        {
            if (!functions.TryGetValue(method, out string funcName) && method.IsDefined<FunctionBindAttribute>())
            {
                var attr = method.GetCustomAttributes<FunctionBindAttribute>().FirstOrDefault();
                funcName = attr.FunctionName;
                functions.TryAdd(method, funcName);
            }

            return funcName;
        }

        /// <summary>
        /// 获取方法调用的绑定。
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static IMethodCallBinder GetMethodBinder(MethodInfo method)
        {
            foreach (var kvp in matchBinders)
            {
                if (kvp.Key.Invoke(method))
                {
                    return matchBinders[kvp.Key];
                }
            }

            if (!defBinders.TryGetValue(method, out IMethodCallBinder binder) && method.IsDefined<MethodCallBindAttribute>())
            {
                var attr = method.GetCustomAttributes<MethodCallBindAttribute>().FirstOrDefault();
                if (attr.BinderType == null)
                {
                    return null;
                }

                binder = attr.BinderType.New<IMethodCallBinder>();
                if (binder == null)
                {
                    throw new ArgumentException(SR.GetString(SRKind.ClassNotImplInterface, "IMethodCallBinder"));
                }

                defBinders.TryAdd(method, binder);
            }

            return binder;
        }
    }
}
