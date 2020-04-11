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
using Fireasy.Common.Reflection;
using Fireasy.Common.Security;
using Fireasy.Data.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体代理编译器。
    /// </summary>
    public static class EntityProxyManager
    {
        private static readonly SafetyDictionary<EntityAssemblyKey, Assembly> cache = new SafetyDictionary<EntityAssemblyKey, Assembly>(new EntityAssemblyKeyEqualityComparer());

        private class EntityAssemblyKey
        {
            public Type ContextType { get; set; }

            public Assembly Assembly { get; set; }
        }

        private class EntityAssemblyKeyEqualityComparer : IEqualityComparer<EntityAssemblyKey>
        {
            public bool Equals(EntityAssemblyKey x, EntityAssemblyKey y)
            {
                return x.ContextType == y.ContextType && x.Assembly == y.Assembly;
            }

            public int GetHashCode(EntityAssemblyKey obj)
            {
                return -1;
            }
        }

        /// <summary>
        /// 获取类型的代理类型。
        /// </summary>
        /// <param name="aware"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static Type GetType(IContextTypeAware aware, Type entityType)
        {
            return aware != null ? GetType(aware.ContextType, entityType) : null;
        }

        /// <summary>
        /// 获取类型的代理类型。
        /// </summary>
        /// <param name="contextType"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static Type GetType(Type contextType, Type entityType)
        {
            if (!entityType.IsEntityType())
            {
                throw new ArgumentException();
            }

            if (!entityType.IsNotCompiled())
            {
                return entityType;
            }

            if (contextType == null)
            {
                return ReflectionCache.GetMember("EntityProxyType", entityType, cache, (k, c) =>
                {
                    foreach (var assm in c.Values)
                    {
                        var proxyType = assm.GetTypes().FirstOrDefault(t => k.IsAssignableFrom(t));
                        if (proxyType != null)
                        {
                            return proxyType;
                        }
                    }

                    return k;
                });
            }

            return ReflectionCache.GetMember("EntityProxyType", entityType, contextType, (k, c) =>
            {
                var assembly = CompileAll(c, k.Assembly, null, null);
                return assembly == null ? k : assembly.GetType(k.Name, true);
            });
        }

        /// <summary>
        /// 编译程序集中的所有类型。
        /// </summary>
        /// <param name="contextType">当前上下文实例的类型。</param>
        /// <param name="assembly">当前的程序集。</param>
        /// <param name="types">指定需要编译的类型，如果为 null 则遍列 <paramref name="assembly"/> 中的所有可导出类型。</param>
        /// <param name="injection">用来向实体类中注入代码。</param>
        /// <returns></returns>
        public static Assembly CompileAll(Type contextType, Assembly assembly, Type[] types, IInjectionProvider injection)
        {
            var assemblyKey = new EntityAssemblyKey { ContextType = contextType, Assembly = assembly };
            return cache.GetOrAdd(assemblyKey, key =>
                {
                    var rndNo = RandomGenerator.Create();
                    var assemblyName = string.Concat(key.Assembly.GetName().Name, ".", rndNo);
                    var assemblyBuilder = new DynamicAssemblyBuilder(assemblyName);

                    types ??= key.Assembly.GetExportedTypes();

                    types.Where(s => s.IsNotCompiled() && !s.IsSealed)
                        .ForEach(s => EntityProxyBuilder.BuildType(s, null, assemblyBuilder, injection));

                    return assemblyBuilder.AssemblyBuilder;
                });
        }
    }
}
