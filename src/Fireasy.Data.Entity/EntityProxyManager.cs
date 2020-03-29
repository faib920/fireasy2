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
            public string ProviderName { get; set; }

            public Assembly Assembly { get; set; }
        }

        private class EntityAssemblyKeyEqualityComparer : IEqualityComparer<EntityAssemblyKey>
        {
            public bool Equals(EntityAssemblyKey x, EntityAssemblyKey y)
            {
                return x.ProviderName == y.ProviderName && x.Assembly == y.Assembly;
            }

            public int GetHashCode(EntityAssemblyKey obj)
            {
                return -1;
            }
        }

        /// <summary>
        /// 获取类型的代理类型。
        /// </summary>
        /// <param name="providerAware"></param>
        /// <returns></returns>
        public static Type GetType(IProviderAware providerAware, Type entityType)
        {
            return GetType(providerAware == null ? (string)null : providerAware.Provider.ProviderName, entityType);
        }

        /// <summary>
        /// 获取类型的代理类型。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static Type GetType(IProvider provider, Type entityType)
        {
            return GetType(provider?.ProviderName, entityType);
        }

        /// <summary>
        /// 获取类型的代理类型。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static Type GetType(string providerName, Type entityType)
        {
            if (!entityType.IsEntityType())
            {
                throw new ArgumentException();
            }

            if (!entityType.IsNotCompiled())
            {
                return entityType;
            }

            if (string.IsNullOrEmpty(providerName))
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

            return ReflectionCache.GetMember("EntityProxyType", entityType, providerName, (k, n) =>
            {
                var assembly = CompileAll(n, k.Assembly, null, null);
                return assembly == null ? k : assembly.GetType(k.Name, true);
            });
        }

        /// <summary>
        /// 编译程序集中的所有类型。
        /// </summary>
        /// <param name="providerName">当前的提供者名称。</param>
        /// <param name="assembly">当前的程序集。</param>
        /// <param name="types">指定需要编译的类型，如果为 null 则遍列 <paramref name="assembly"/> 中的所有可导出类型。</param>
        /// <param name="injection"></param>
        /// <returns></returns>
        public static Assembly CompileAll(string providerName, Assembly assembly, Type[] types, IInjectionProvider injection)
        {
            var assemblyKey = new EntityAssemblyKey { ProviderName = providerName, Assembly = assembly };
            return cache.GetOrAdd(assemblyKey, key =>
                {
                    var assemblyName = string.Concat(key.Assembly.GetName().Name, ".", key.ProviderName ?? "Dynamic");
                    var assemblyBuilder = new DynamicAssemblyBuilder(assemblyName);

                    types ??= key.Assembly.GetExportedTypes();

                    types.Where(s => s.IsNotCompiled())
                        .ForEach(s => EntityProxyBuilder.BuildType(s, null, assemblyBuilder, injection));

                    return assemblyBuilder.AssemblyBuilder;
                });
        }
    }
}
