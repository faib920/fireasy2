// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NET35
using Fireasy.Common.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Fireasy.Common.Dynamic
{
    public static class BinderWrapper
    {
        private static string CSharpAssemblyName = typeof(RuntimeBinderException).Assembly.FullName;

        private static string BinderTypeName = $"{typeof(Binder).FullName}, " + CSharpAssemblyName;
        private static string CSharpArgumentInfoTypeName = $"{typeof(CSharpArgumentInfo).FullName}, " + CSharpAssemblyName;
        private static string CSharpArgumentInfoFlagsTypeName = $"{typeof(CSharpArgumentInfoFlags).AssemblyQualifiedName}, " + CSharpAssemblyName;
        private static string CSharpBinderFlagsTypeName = $"{typeof(CSharpBinderFlags).FullName}, " + CSharpAssemblyName;

        private static object getCSharpArgumentInfoArray;
        private static object setCSharpArgumentInfoArray;
        private static MethodInvoker getMemberCall;
        private static MethodInvoker setMemberCall;
        private static bool initialized;

        private static void Init()
        {
            if (!initialized)
            {
                var binderType = Type.GetType(BinderTypeName, false);
                if (binderType == null)
                {
                    throw new InvalidOperationException(string.Format("Could not resolve type '{0}'. You may need to add a reference to Microsoft.CSharp.dll to work with dynamic types.", BinderTypeName));
                }

                getCSharpArgumentInfoArray = CreateSharpArgumentInfoArray(0);
                setCSharpArgumentInfoArray = CreateSharpArgumentInfoArray(0, 3);
                CreateMemberCalls();

                initialized = true;
            }
        }

        private static object CreateSharpArgumentInfoArray(params int[] values)
        {
            var csharpArgumentInfoType = Type.GetType(CSharpArgumentInfoTypeName);
            var csharpArgumentInfoFlags = Type.GetType(CSharpArgumentInfoFlagsTypeName);

            var a = Array.CreateInstance(csharpArgumentInfoType, values.Length);

            for (var i = 0; i < values.Length; i++)
            {
                var createArgumentInfoMethod = csharpArgumentInfoType.GetMethod(nameof(CSharpArgumentInfo.Create), new[] { csharpArgumentInfoFlags, typeof(string) });
                var arg = createArgumentInfoMethod.Invoke(null, new object[] { 0, null });
                a.SetValue(arg, i);
            }

            return a;
        }

        private static void CreateMemberCalls()
        {
            var csharpArgumentInfoType = Type.GetType(CSharpArgumentInfoTypeName, true);
            var csharpBinderFlagsType = Type.GetType(CSharpBinderFlagsTypeName, true);
            var binderType = Type.GetType(BinderTypeName, true);

            var csharpArgumentInfoTypeEnumerableType = typeof(IEnumerable<>).MakeGenericType(csharpArgumentInfoType);

            var getMemberMethod = binderType.GetMethod(nameof(Binder.GetMember), new[] { csharpBinderFlagsType, typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType });
            getMemberCall = ReflectionCache.GetInvoker(getMemberMethod);

            var setMemberMethod = binderType.GetMethod(nameof(Binder.SetMember), new[] { csharpBinderFlagsType, typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType });
            setMemberCall = ReflectionCache.GetInvoker(setMemberMethod);
        }

        public static CallSiteBinder GetMember(string name)
        {
            Init();
            return (CallSiteBinder)getMemberCall.Invoke(null, 0, name, typeof(BinderWrapper), getCSharpArgumentInfoArray);
        }

        public static CallSiteBinder SetMember(string name)
        {
            Init();
            return (CallSiteBinder)setMemberCall.Invoke(null, 0, name, typeof(BinderWrapper), setCSharpArgumentInfoArray);
        }
    }
}

#endif