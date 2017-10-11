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

        private static string BinderTypeName = "Microsoft.CSharp.RuntimeBinder.Binder, " + CSharpAssemblyName;
        private static string CSharpArgumentInfoTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo, " + CSharpAssemblyName;
        private static string CSharpArgumentInfoFlagsTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags, " + CSharpAssemblyName;
        private static string CSharpBinderFlagsTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags, " + CSharpAssemblyName;

        private static object _getCSharpArgumentInfoArray;
        private static object _setCSharpArgumentInfoArray;
        private static MethodInvoker _getMemberCall;
        private static MethodInvoker _setMemberCall;
        private static bool _init;

        private static void Init()
        {
            if (!_init)
            {
                var binderType = Type.GetType(BinderTypeName, false);
                if (binderType == null)
                {
                    throw new InvalidOperationException(string.Format("Could not resolve type '{0}'. You may need to add a reference to Microsoft.CSharp.dll to work with dynamic types.", BinderTypeName));
                }

                _getCSharpArgumentInfoArray = CreateSharpArgumentInfoArray(0);
                _setCSharpArgumentInfoArray = CreateSharpArgumentInfoArray(0, 3);
                CreateMemberCalls();

                _init = true;
            }
        }

        private static object CreateSharpArgumentInfoArray(params int[] values)
        {
            var csharpArgumentInfoType = Type.GetType(CSharpArgumentInfoTypeName);
            var csharpArgumentInfoFlags = Type.GetType(CSharpArgumentInfoFlagsTypeName);

            var a = Array.CreateInstance(csharpArgumentInfoType, values.Length);

            for (var i = 0; i < values.Length; i++)
            {
                var createArgumentInfoMethod = csharpArgumentInfoType.GetMethod("Create", new[] { csharpArgumentInfoFlags, typeof(string) });
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

            var getMemberMethod = binderType.GetMethod("GetMember", new[] { csharpBinderFlagsType, typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType });
            _getMemberCall = ReflectionCache.GetInvoker(getMemberMethod);

            var setMemberMethod = binderType.GetMethod("SetMember", new[] { csharpBinderFlagsType, typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType });
            _setMemberCall = ReflectionCache.GetInvoker(setMemberMethod);
        }

        public static CallSiteBinder GetMember(string name)
        {
            Init();
            return (CallSiteBinder)_getMemberCall.Invoke(null, 0, name, typeof(BinderWrapper), _getCSharpArgumentInfoArray);
        }

        public static CallSiteBinder SetMember(string name)
        {
            Init();
            return (CallSiteBinder)_setMemberCall.Invoke(null, 0, name, typeof(BinderWrapper), _setCSharpArgumentInfoArray);
        }
    }
}

#endif