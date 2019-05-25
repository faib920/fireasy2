// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Emit;
using Fireasy.Common.Extensions;
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
        private static MethodInfo MthTypeGetTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Public | BindingFlags.Static);
        private static MethodInfo MthGetProperty = typeof(PropertyUnity).GetMethod(nameof(PropertyUnity.GetProperty));
        private static MethodInfo MthGetValue = typeof(ProtectedEntity).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(s => s.Name == "ProtectGetValue");
        private static MethodInfo MthSetValue = typeof(ProtectedEntity).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(s => s.Name == "ProtectSetValue");
        private static MethodInfo MthInitValue = typeof(ProtectedEntity).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(s => s.Name == "ProtectInitializeValue");
        private static MethodInfo MthPVNewValue = typeof(PropertyValue).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(s => s.Name == "NewValue");
        private static MethodInfo MthPVGetValue = typeof(PropertyValue).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(s => s.Name == "GetValue" && s.IsGenericMethod);

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
                    var op_Explicit = typeof(PropertyValue).GetMethods().FirstOrDefault(s => s.Name == "op_Explicit" && s.ReturnType == opType);
                    propertyBuilder.DefineGetMethod(ilCoding: code =>
                        {
                            code.Emitter.DeclareLocal(typeof(PropertyValue));
                            code.Emitter.DeclareLocal(get.ReturnType);
                            code.Emitter
                            .nop
                            .ldarg_0
                            .ldtoken(entityType)
                            .call(MthTypeGetTypeFromHandle)
                            .ldstr(property.Name)
                            .call(MthGetProperty)
                            .call(MthGetValue)
                            .Assert(op_Explicit != null, e1 => e1.call(op_Explicit), 
                                e1 => e1.call(MthPVGetValue.MakeGenericMethod(property.PropertyType)))
                            .Assert(isEnum, e1 => e1.unbox_any(property.PropertyType))
                            .stloc_1
                            .ldloc_1
                            .ret();
                        });
                }

                if (set != null)
                {
                    var op_Implicit = typeof(PropertyValue).GetMethods().FirstOrDefault(s => s.Name == "op_Implicit" && s.GetParameters()[0].ParameterType == opType);
                    propertyBuilder.DefineSetMethod(ilCoding: code =>
                        {
                            code.Emitter
                            .nop
                            .ldarg_0
                            .ldtoken(entityType)
                            .call(MthTypeGetTypeFromHandle)
                            .ldstr(property.Name)
                            .call(MthGetProperty)
                            .ldarg_1
                            .Assert(isEnum, e1 => e1.box(property.PropertyType))
                            .Assert(op_Implicit != null, e1 => e1.call(op_Implicit), 
                                e1 => e1.ldtoken(property.PropertyType).call(MthTypeGetTypeFromHandle).call(MthPVNewValue))
                            .call(MthSetValue)
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
                            callvirt(MthInitValue).ret();
                    });

            injection?.Inject(entityType, assemblyBuilder, typeBuilder);

            return typeBuilder.CreateType();
        }
    }
}
