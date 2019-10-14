// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Fireasy.Common.Dynamic
{
    public static class BinderWrapper
    {
        private static object getCSharpArgumentInfoArray;
        private static object setCSharpArgumentInfoArray;
        private static MethodInvoker getMemberCall;
        private static MethodInvoker setMemberCall;
        private static bool initialized;

        private static void Init()
        {
            if (!initialized)
            {
                getCSharpArgumentInfoArray = CreateSharpArgumentInfoArray(0);
                setCSharpArgumentInfoArray = CreateSharpArgumentInfoArray(0, 3);
                CreateMemberCalls();

                initialized = true;
            }
        }

        private static object CreateSharpArgumentInfoArray(params int[] values)
        {
            var a = Array.CreateInstance(typeof(CSharpArgumentInfo), values.Length);

            for (var i = 0; i < values.Length; i++)
            {
                var createArgumentInfoMethod = typeof(CSharpArgumentInfo).GetMethod(nameof(CSharpArgumentInfo.Create), new[] { typeof(CSharpArgumentInfoFlags), typeof(string) });
                var arg = createArgumentInfoMethod.Invoke(null, new object[] { 0, null });
                a.SetValue(arg, i);
            }

            return a;
        }

        private static void CreateMemberCalls()
        {
            var csharpArgumentInfoTypeEnumerableType = typeof(IEnumerable<>).MakeGenericType(typeof(CSharpArgumentInfo));

            var getMemberMethod = typeof(Binder).GetMethod(nameof(Binder.GetMember), new[] { typeof(CSharpBinderFlags), typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType });
            getMemberCall = ReflectionCache.GetInvoker(getMemberMethod);

            var setMemberMethod = typeof(Binder).GetMethod(nameof(Binder.SetMember), new[] { typeof(CSharpBinderFlags), typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType });
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
