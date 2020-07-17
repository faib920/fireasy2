// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Fireasy.Common.Tasks
{
    internal static class ServiceDescriptorHelper
    {
        private class MethodCache
        {
            internal protected static readonly MethodInfo Singleton = typeof(ServiceDescriptor).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(ServiceDescriptor.Singleton) && s.IsGenericMethod && s.GetGenericArguments().Length == 2 && s.GetParameters().Length == 1);
        }

        internal static ServiceDescriptor CreateSingleton(Type serviceType, Type implType, Func<Delegate> creator)
        {
            var method = MethodCache.Singleton.MakeGenericMethod(serviceType, implType);
            return (ServiceDescriptor)method.FastInvoke(null, new[] { creator() });
        }
    }
}
#endif