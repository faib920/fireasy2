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
    public class TranslateUtils
    {
        private static ConcurrentDictionary<MethodInfo, IMethodCallBinder> binders = new ConcurrentDictionary<MethodInfo, IMethodCallBinder>();
        private static ConcurrentDictionary<MethodInfo, string> functions = new ConcurrentDictionary<MethodInfo, string>();

        /// <summary>
        /// 添加方法调用的绑定。
        /// </summary>
        /// <param name="method"></param>
        /// <param name="binder"></param>
        public static void AddMethodBinder(MethodInfo method, IMethodCallBinder binder)
        {
            binders.TryAdd(method, binder);
        }

        /// <summary>
        /// 获取自定义的函数。
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string GetCustomFunction(MethodInfo method)
        {
            if (!functions.TryGetValue(method, out string funcName))
            {
                var attr = method.GetCustomAttributes<FunctionBindAttribute>().FirstOrDefault();
                if (attr != null)
                {
                    funcName = attr.FunctionName;
                    functions.TryAdd(method, funcName);
                }
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
            if (!binders.TryGetValue(method, out IMethodCallBinder binder))
            {
                var attr = method.GetCustomAttributes<MethodCallBindAttribute>().FirstOrDefault();
                if (attr != null)
                {
                    if (attr.BinderType == null)
                    {
                        return null;
                    }

                    binder = attr.BinderType.New<IMethodCallBinder>();
                    if (binder == null)
                    {
                        throw new ArgumentException(SR.GetString(SRKind.ClassNotImplInterface, "IMethodCallBinder"));
                    }

                    binders.TryAdd(method, binder);
                }
            }

            return binder;
        }
    }
}
