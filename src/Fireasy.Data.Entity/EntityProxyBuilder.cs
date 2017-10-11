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
        private static MethodInfo mthTypeGetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
        private static MethodInfo mthGetProperty = typeof(PropertyUnity).GetMethod("GetProperty");
        private static MethodInfo mthGetValue = typeof(ProtectedEntity).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(s => s.Name == "ProtectGetValue");
        private static MethodInfo mthSetValue = typeof(ProtectedEntity).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(s => s.Name == "ProtectSetValue");
        private static MethodInfo mthPVNewValue = typeof(PropertyValue).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(s => s.Name == "NewValue");
        private static MethodInfo mthPVGetValue = typeof(PropertyValue).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(s => s.Name == "GetValue");

        /// <summary>
        /// 构造实体类 <paramref name="entityType"/> 的代理类。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="assemblyBuilder"></param>
        /// <returns></returns>
        public static Type BuildType(Type entityType, DynamicAssemblyBuilder assemblyBuilder = null)
        {
            var typeBuilder = assemblyBuilder.DefineType(entityType.Name, baseType: entityType);
            typeBuilder.ImplementInterface(typeof(ICompiledEntity));

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
                            code.Emitter.DeclareLocal(get.ReturnType);
                            code.Emitter
                            .nop
                            .ldarg_0
                            .ldtoken(entityType)
                            .call(mthTypeGetTypeFromHandle)
                            .ldstr(property.Name)
                            .call(mthGetProperty)
                            .call(mthGetValue)
                            .Assert(op_Explicit != null, e1 => e1.call(op_Explicit), e1 => e1.call(mthPVGetValue).isinst(opType))
                            .Assert(isEnum, e1 => e1.unbox_any(property.PropertyType))
                            .stloc_0
                            .ldloc_0
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
                            .call(mthTypeGetTypeFromHandle)
                            .ldstr(property.Name)
                            .call(mthGetProperty)
                            .ldarg_1
                            .Assert(isEnum, e1 => e1.box(property.PropertyType))
                            .Assert(op_Implicit != null, e1 => e1.call(op_Implicit), e1 => e1.ldnull.call(mthPVNewValue))
                            .call(mthSetValue)
                            .nop
                            .ret();
                        });
                }
            }

            return typeBuilder.CreateType();
        }
    }
}
