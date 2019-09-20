// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Emit;
using Fireasy.Common.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体代理编译器。
    /// </summary>
    public static class EntityProxyManager
    {
        private static SafetyDictionary<string, Assembly> cache = new SafetyDictionary<string, Assembly>();

        /// <summary>
        /// 获取类型的代理类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetType(Type type)
        {
            if (cache.TryGetValue(type.Assembly.FullName, out Assembly assembly))
            {
                return assembly.GetType(type.Name);
            }

            assembly = cache.GetOrAdd(type.FullName, () =>
                {
                    var assemblyName = string.Concat(type.FullName, "_Dynamic");
                    var assemblyBuilder = new DynamicAssemblyBuilder(assemblyName);
                    EntityProxyBuilder.BuildType(type, null, assemblyBuilder, null);
                    return assemblyBuilder.AssemblyBuilder;
                });

            return assembly.GetType(type.Name);
        }

        public static Assembly CompileAll(Assembly assembly, IInjectionProvider injection)
        {
            return cache.GetOrAdd(assembly.FullName, () =>
                {
                    var assemblyName = string.Concat(assembly.GetName().Name, "_Dynamic");
                    var assemblyBuilder = new DynamicAssemblyBuilder(assemblyName);

                    var ss = assembly.GetExportedTypes()
                        .Where(s => s.IsNotCompiled()).ToList();
                    assembly.GetExportedTypes()
                        .Where(s => s.IsNotCompiled())
                        .ForEach(s => EntityProxyBuilder.BuildType(s, null, assemblyBuilder, injection));

                    return assemblyBuilder.AssemblyBuilder;
                });
        }
    }
}
