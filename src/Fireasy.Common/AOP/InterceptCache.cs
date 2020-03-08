// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Reflection;
using Fireasy.Common.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Fireasy.Common.Aop
{
    /// <summary>
    /// 方法的缓存。
    /// </summary>
    internal sealed class InterceptCache
    {
        internal static MethodInfo TypeGetMethod = typeof(Type).GetMethod(nameof(Type.GetMethod), new[] { typeof(string) });
        internal static MethodInfo MethodGetCurrent = typeof(MethodBase).GetMethod(nameof(MethodBase.GetCurrentMethod), BindingFlags.Public | BindingFlags.Static);
        internal static MethodInfo MethodGetBaseDefinition = typeof(MethodInfo).GetMethod(nameof(MethodInfo.GetBaseDefinition));
        internal static MethodInfo TypeGetProperty = typeof(Type).GetMethod(nameof(Type.GetProperty), new[] { typeof(string) });
        internal static MethodInfo MemberGetCustomAttributes = typeof(MemberInfo).GetMethod(nameof(MemberInfo.GetCustomAttributes), new[] { typeof(Type), typeof(bool) });
        internal static MethodInfo TypeGetTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Public | BindingFlags.Static);
        internal static MethodInfo CallInfoGetMember = typeof(InterceptCallInfo).GetMethod($"get_{nameof(InterceptCallInfo.Member)}");
        internal static MethodInfo CallInfoSetMember = typeof(InterceptCallInfo).GetMethod($"set_{nameof(InterceptCallInfo.Member)}");
        internal static MethodInfo CallInfoSetTarget = typeof(InterceptCallInfo).GetMethod($"set_" + nameof(InterceptCallInfo.Target));
        internal static MethodInfo CallInfoSetReturnType = typeof(InterceptCallInfo).GetMethod($"set_" + nameof(InterceptCallInfo.ReturnType));
        internal static MethodInfo CallInfoSetDefinedType = typeof(InterceptCallInfo).GetMethod($"set_{nameof(InterceptCallInfo.DefinedType)}");
        internal static MethodInfo CallInfoSetArguments = typeof(InterceptCallInfo).GetMethod($"set_{nameof(InterceptCallInfo.Arguments)}");
        internal static MethodInfo CallInfoGetArguments = typeof(InterceptCallInfo).GetMethod($"get_{nameof(InterceptCallInfo.Arguments)}");
        internal static MethodInfo CallInfoGetCancel = typeof(InterceptCallInfo).GetMethod($"get_{nameof(InterceptCallInfo.Cancel)}");
        internal static MethodInfo CallInfoSetReturnValue = typeof(InterceptCallInfo).GetMethod($"set_{nameof(InterceptCallInfo.ReturnValue)}");
        internal static MethodInfo CallInfoGetReturnValue = typeof(InterceptCallInfo).GetMethod($"get_{nameof(InterceptCallInfo.ReturnValue)}");
        internal static MethodInfo CallInfoSetException = typeof(InterceptCallInfo).GetMethod($"set_{nameof(InterceptCallInfo.Exception)}");
        internal static MethodInfo CallInfoSetInterceptType = typeof(InterceptCallInfo).GetMethod($"set_{nameof(InterceptCallInfo.InterceptType)}");
        internal static MethodInfo InterceptorsAdd = typeof(List<IInterceptor>).GetMethod(nameof(List<IInterceptor>.Add));
        internal static MethodInfo InterceptorsGetItem = typeof(List<IInterceptor>).GetMethod($"get_Item");
        internal static MethodInfo InterceptorsGetCount = typeof(List<IInterceptor>).GetMethod($"get_Count");
        internal static MethodInfo InterceptorIntercept = typeof(IInterceptor).GetMethod(nameof(IInterceptor.Intercept));
        internal static MethodInfo InterceptorInitialize = typeof(IInterceptor).GetMethod(nameof(IInterceptor.Initialize));
        internal static MethodInfo InterceptContextSetAttribute = typeof(InterceptContext).GetMethod($"set_{nameof(InterceptContext.Attribute)}");
        internal static MethodInfo InterceptContextSetTarget = typeof(InterceptContext).GetMethod($"set_{nameof(InterceptContext.Target)}");
        internal static MethodInfo AttributesAdd = typeof(List<InterceptAttribute>).GetMethod(nameof(List<InterceptAttribute>.Add));
        internal static MethodInfo AttributesGetItem = typeof(List<InterceptAttribute>).GetMethod($"get_Item");
        internal static MethodInfo GetDefaultValue = typeof(ReflectionExtension).GetMethod(nameof(ReflectionExtension.GetDefaultValue));
        internal static MethodInfo AllGetAttributes = typeof(AspectUtil).GetMethod(nameof(AspectUtil.GetInterceptAttributes));
        internal static MethodInfo TaskFromResult = typeof(Task).GetMethod(nameof(Task.FromResult));
    }

    /// <summary>
    /// 辅助类。
    /// </summary>
    public static class AspectUtil
    {
        /// <summary>
        /// 获取所有拦截特性。
        /// </summary>
        /// <param name="callInfo"></param>
        /// <returns></returns>
        public static InterceptAttribute[] GetInterceptAttributes(InterceptCallInfo callInfo)
        {
            return callInfo.Member.GetCustomAttributes<InterceptAttribute>(true)
                .Union(callInfo.DefinedType.GetCustomAttributes<InterceptAttribute>(true))
                .ToArray();
        }
    }
}
