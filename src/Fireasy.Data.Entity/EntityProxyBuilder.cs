// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Emit;
using Fireasy.Common.Extensions;
using Fireasy.Common.Reflection;
using System;
using System.Linq;
using System.Reflection;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体类代理类构造器。
    /// </summary>
    public class EntityProxyBuilder
    {
        private class MethodCache
        {
            internal protected static MethodInfo TypeGetTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Public | BindingFlags.Static);
            internal protected static MethodInfo GetProperty = typeof(PropertyUnity).GetMethod(nameof(PropertyUnity.GetProperty));
            internal protected static MethodInfo GetValue = typeof(ProtectedEntity).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(s => s.Name == "ProtectGetValue");
            internal protected static MethodInfo SetValue = typeof(ProtectedEntity).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(s => s.Name == "ProtectSetValue");
            internal protected static MethodInfo InitValue = typeof(ProtectedEntity).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(s => s.Name == "ProtectInitializeValue");
            internal protected static MethodInfo PVNewValue = typeof(PropertyValue).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(s => s.Name == "NewValue");
            internal protected static MethodInfo PVGetValue = typeof(PropertyValue).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(s => s.Name == "GetValue" && s.IsGenericMethod);
        }

        /// <summary>
        /// 构造实体类 <paramref name="entityType"/> 的代理类。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="entityName"></param>
        /// <param name="assemblyBuilder"></param>
        /// <param name="injection"></param>
        /// <returns></returns>
        public static Type BuildType(Type entityType, string entityName, DynamicAssemblyBuilder assemblyBuilder = null, IInjectionProvider injection = null)
        {
            var typeBuilder = assemblyBuilder.DefineType(entityName ?? entityType.Name, baseType: entityType);
            typeBuilder.ImplementInterface(typeof(ICompiledEntity));
            typeBuilder.SetCustomAttribute(() => new SerializableAttribute());

            var properties = from s in entityType.GetProperties()
                             let getMth = s.GetGetMethod()
                             where getMth.IsVirtual && !getMth.IsFinal
                             select s;

            foreach (var property in properties)
            {
                var propertyBuilder = typeBuilder.DefineProperty(property.Name, property.PropertyType);

                var get = property.GetGetMethod();
                var set = property.GetSetMethod();
                var isEnum = property.PropertyType.GetNonNullableType().IsEnum;
                var opType = isEnum ? typeof(Enum) : property.PropertyType;

                if (get != null)
                {
                    var op_Explicit = ReflectionCache.GetMember("PropertyValue_Explicit", opType, "op_Explicit", (k, n) => typeof(PropertyValue).GetMethods().FirstOrDefault(s => s.Name == n && s.ReturnType == k));
                    propertyBuilder.DefineGetMethod(ilCoding: code =>
                        {
                            code.Emitter.DeclareLocal(typeof(PropertyValue));
                            code.Emitter.DeclareLocal(get.ReturnType);
                            code.Emitter
                            .nop
                            .ldarg_0
                            .ldtoken(entityType)
                            .call(MethodCache.TypeGetTypeFromHandle)
                            .ldstr(property.Name)
                            .call(MethodCache.GetProperty)
                            .call(MethodCache.GetValue)
                            .Assert(op_Explicit != null, e1 => e1.call(op_Explicit), 
                                e1 => e1.call(MethodCache.PVGetValue.MakeGenericMethod(property.PropertyType)))
                            .Assert(isEnum, e1 => e1.unbox_any(property.PropertyType))
                            .stloc_1
                            .ldloc_1
                            .ret();
                        });
                }

                if (set != null)
                {
                    var op_Implicit = ReflectionCache.GetMember("PropertyValue_Implicit", opType, "op_Implicit", (k, n) => typeof(PropertyValue).GetMethods().FirstOrDefault(s => s.Name == n && s.GetParameters()[0].ParameterType == k));
                    propertyBuilder.DefineSetMethod(ilCoding: code =>
                        {
                            code.Emitter
                            .nop
                            .ldarg_0
                            .ldtoken(entityType)
                            .call(MethodCache.TypeGetTypeFromHandle)
                            .ldstr(property.Name)
                            .call(MethodCache.GetProperty)
                            .ldarg_1
                            .Assert(isEnum, e1 => e1.box(property.PropertyType))
                            .Assert(op_Implicit != null, e1 => e1.call(op_Implicit), 
                                e1 => e1.ldtoken(property.PropertyType).call(MethodCache.TypeGetTypeFromHandle).call(MethodCache.PVNewValue))
                            .call(MethodCache.SetValue)
                            .nop
                            .ret();
                        });
                }
            }

            var initMethod = typeBuilder.DefineMethod("InitializeValue",
                parameterTypes: new[] { typeof(IProperty), typeof(PropertyValue) },
                ilCoding: code =>
                    {
                        code.Emitter.
                            ldarg_0.ldarg_1.ldarg_2.
                            callvirt(MethodCache.InitValue).ret();
                    });

            injection?.Inject(new EntityInjectionContext 
                { 
                    EntityType = entityType, 
                    AssemblyBuilder = assemblyBuilder, 
                    TypeBuilder = typeBuilder 
                });

            return typeBuilder.CreateType();
        }
    }
}
