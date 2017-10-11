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

namespace Fireasy.Common.Aop
{
    internal sealed class InterceptCache
    {
        internal static MethodInfo TypeGetMethod = typeof(Type).GetMethod("GetMethod", new[] { typeof(string) });
        internal static MethodInfo MethodGetCurrent = typeof(MethodBase).GetMethod("GetCurrentMethod", BindingFlags.Public | BindingFlags.Static);
        internal static MethodInfo MethodGetBaseDefinition = typeof(MethodInfo).GetMethod("GetBaseDefinition");
        internal static MethodInfo TypeGetProperty = typeof(Type).GetMethod("GetProperty", new[] { typeof(string) });
        internal static MethodInfo MemberGetCustomAttributes = typeof(MemberInfo).GetMethod("GetCustomAttributes", new[] { typeof(Type), typeof(bool) });
        internal static MethodInfo TypeGetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
        internal static MethodInfo CallInfoGetMember = typeof(InterceptCallInfo).GetMethod("get_Member");
        internal static MethodInfo CallInfoSetMember = typeof(InterceptCallInfo).GetMethod("set_Member");
        internal static MethodInfo CallInfoSetTarget = typeof(InterceptCallInfo).GetMethod("set_Target");
        internal static MethodInfo CallInfoSetDefinedType = typeof(InterceptCallInfo).GetMethod("set_DefinedType");
        internal static MethodInfo CallInfoSetArguments = typeof(InterceptCallInfo).GetMethod("set_Arguments");
        internal static MethodInfo CallInfoGetArguments = typeof(InterceptCallInfo).GetMethod("get_Arguments");
        internal static MethodInfo CallInfoGetCancel = typeof(InterceptCallInfo).GetMethod("get_Cancel");
        internal static MethodInfo CallInfoSetReturnValue = typeof(InterceptCallInfo).GetMethod("set_ReturnValue");
        internal static MethodInfo CallInfoGetReturnValue = typeof(InterceptCallInfo).GetMethod("get_ReturnValue");
        internal static MethodInfo CallInfoSetException = typeof(InterceptCallInfo).GetMethod("set_Exception");
        internal static MethodInfo CallInfoSetInterceptType = typeof(InterceptCallInfo).GetMethod("set_InterceptType");
        internal static MethodInfo InterceptorsAdd = typeof(List<IInterceptor>).GetMethod("Add");
        internal static MethodInfo InterceptorsGetItem = typeof(List<IInterceptor>).GetMethod("get_Item");
        internal static MethodInfo InterceptorsGetCount = typeof(List<IInterceptor>).GetMethod("get_Count");
        internal static MethodInfo InterceptorIntercept = typeof(IInterceptor).GetMethod("Intercept");
        internal static MethodInfo InterceptorInitialize = typeof(IInterceptor).GetMethod("Initialize");
        internal static MethodInfo InterceptContextSetAttribute = typeof(InterceptContext).GetMethod("set_Attribute");
        internal static MethodInfo InterceptContextSetTarget = typeof(InterceptContext).GetMethod("set_Target");
        internal static MethodInfo AttributesAdd = typeof(List<InterceptAttribute>).GetMethod("Add");
        internal static MethodInfo AttributesGetItem = typeof(List<InterceptAttribute>).GetMethod("get_Item");
        internal static MethodInfo MethodGetDefaultValue = typeof(ReflectionExtension).GetMethod("GetDefaultValue");
        internal static MethodInfo MethodAllGetAttributes = typeof(AspectUtil).GetMethod("GetInterceptAttributes");
    }

    public class AspectUtil
    {
        public static InterceptAttribute[] GetInterceptAttributes(InterceptCallInfo callInfo)
        {
            var list = callInfo.Member.GetCustomAttributes<InterceptAttribute>(true)
                .Union(callInfo.DefinedType.GetCustomAttributes<InterceptAttribute>(true))
                .ToArray();
            return list;
        }
    }
}
