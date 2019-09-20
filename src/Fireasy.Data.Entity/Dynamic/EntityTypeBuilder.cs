// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Emit;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Fireasy.Data.Entity.Dynamic
{
    /// <summary>
    /// 提供动态构造实体的生成器。无法继承此类。
    /// </summary>
    public sealed class EntityTypeBuilder
    {
        private static readonly MethodInfo GetValueMethod = typeof(EntityObject).GetMethods().FirstOrDefault(s => s.Name == nameof(IEntity.GetValue));
        private static readonly MethodInfo SetValueMethod = typeof(EntityObject).GetMethods().FirstOrDefault(s => s.Name == nameof(IEntity.SetValue));
        private static readonly MethodInfo SGetValueMethod = typeof(PropertyValue).GetMethods().FirstOrDefault(s => s.Name == nameof(PropertyValue.GetValue) && s.IsGenericMethod);
        private static readonly MethodInfo SNewValueMethod = typeof(PropertyValue).GetMethods().FirstOrDefault(s => s.Name == nameof(PropertyValue.NewValue));
        private static readonly MethodInfo RegisterMethod = typeof(PropertyUnity).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(s => s.Name == nameof(PropertyUnity.RegisterProperty) && !s.IsGenericMethod && s.GetParameters().Length == 2);
        private static readonly MethodInfo TypeGetTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Public | BindingFlags.Static);

        private readonly DynamicAssemblyBuilder assemblyBuilder;
        private List<DynamicFieldBuilder> fields;
        private Dictionary<IProperty, List<Expression<Func<Attribute>>>> validations;
        private List<Expression<Func<Attribute>>> eValidations;

        /// <summary>
        /// 初始化 <see cref="EntityTypeBuilder"/> 类的新实例。
        /// </summary>
        /// <param name="typeName">动态类的类型名称。</param>
        /// <param name="assemblyBuilder">一个 <see cref="DynamicAssemblyBuilder"/> 容器。</param>
        /// <param name="baseType">所要继承的抽象类型，默认为 <see cref="EntityObject"/> 类。</param>
        public EntityTypeBuilder(string typeName, DynamicAssemblyBuilder assemblyBuilder = null, Type baseType = null)
        {
            TypeName = typeName;
            this.assemblyBuilder = assemblyBuilder ?? new DynamicAssemblyBuilder("<DynamicType>_" + typeName);
            InnerBuilder = this.assemblyBuilder.DefineType(TypeName, baseType: baseType ?? typeof(EntityObject));
            InnerBuilder.Creator = () => Create();
            EntityType = InnerBuilder.UnderlyingSystemType;
            Properties = new List<IProperty>();
        }

        /// <summary>
        /// 获取实体类的名称。
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// 获取或设置映射对象。
        /// </summary>
        public EntityMappingAttribute Mapping { get; set; }

        /// <summary>
        /// 获取或设置包含的属性。
        /// </summary>
        public List<IProperty> Properties { get; set; }

        /// <summary>
        /// 获取实体类型。
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// 获取 <see cref="DynamicTypeBuilder"/> 对象。
        /// </summary>
        public DynamicTypeBuilder InnerBuilder { get; private set; }

        /// <summary>
        /// 为属性添加验证规则。
        /// </summary>
        /// <param name="property">要验证的属性。</param>
        /// <param name="attributes">一组 <see cref="ValidationAttribute"/> 特性。</param>
        public void DefineValidateRule(IProperty property, params Expression<Func<Attribute>>[] attributes)
        {
            Guard.ArgumentNull(attributes, nameof(attributes));
            if (validations == null)
            {
                validations = new Dictionary<IProperty, List<Expression<Func<Attribute>>>>();
            }

            var list = validations.TryGetValue(property, () => new List<Expression<Func<Attribute>>>());
            list.AddRange(attributes);
        }

        /// <summary>
        /// 为实体添加验证规则。
        /// </summary>
        /// <param name="attributes">一组 <see cref="ValidationAttribute"/> 特性。</param>
        public void DefineValidateRule(params Expression<Func<Attribute>>[] attributes)
        {
            Guard.ArgumentNull(attributes, nameof(attributes));
            if (eValidations == null)
            {
                eValidations = new List<Expression<Func<Attribute>>>();
            }

            eValidations.AddRange(attributes);
        }

        /// <summary>
        /// 实现多个接口类型。
        /// </summary>
        /// <param name="interfaceTypes"></param>
        public void ImplementInterfaces(params Type[] interfaceTypes)
        {
            if (interfaceTypes != null)
            {
                foreach (var type in interfaceTypes)
                {
                    InnerBuilder.ImplementInterface(type);
                }
            }
        }

        /// <summary>
        /// 为动态类添加特性。
        /// </summary>
        /// <param name="expression">一个特性表达式，必须为 <see cref="NewExpression"/>。</param>
        public void SetCustomAttribute(Expression<Func<Attribute>> expression)
        {
            InnerBuilder.SetCustomAttribute(expression);
        }

        /// <summary>
        /// 创建一个动态的实体类型。
        /// </summary>
        /// <returns></returns>
        public Type Create()
        {
            if (Mapping != null)
            {
                InnerBuilder.SetCustomAttribute<EntityMappingAttribute>(Mapping.TableName);
            }

            fields = new List<DynamicFieldBuilder>();

            DefineProperties(fields);
            DefineConstructors(fields);

            return InnerBuilder.CreateType();
        }

        private string GetFieldName(IProperty property)
        {
            return "<>__" + property.Name;
        }

        private void DefineProperties(ICollection<DynamicFieldBuilder> fields)
        {
            if (Properties == null)
            {
                return;
            }

            foreach (var property in Properties)
            {
                var _property = property;
                var pext = property as PropertyExtension;
                if (pext != null)
                {
                    _property = pext.Property;
                }

                var fieldBuilder = InnerBuilder.DefineField(GetFieldName(_property), typeof(IProperty), null, VisualDecoration.Public, CallingDecoration.Static);
                var propertyBuider = InnerBuilder.DefineProperty(_property.Name, _property.Type, VisualDecoration.Public, CallingDecoration.Virtual);
                var isEnum = _property.Type.IsEnum;
                var opType = isEnum ? typeof(Enum) : _property.Type;

                if (pext != null)
                {
                    pext.GetCustomAttributes().ForEach(s => propertyBuider.SetCustomAttribute(s));
                }

                var op_Explicit = typeof(PropertyValue).GetMethods().FirstOrDefault(s => s.Name == "op_Explicit" && s.ReturnType == _property.Type);
                var op_Implicit = typeof(PropertyValue).GetMethods().FirstOrDefault(s => s.Name == "op_Implicit" && s.GetParameters()[0].ParameterType == _property.Type);

                var getMethod = propertyBuider.DefineGetMethod(calling: CallingDecoration.Virtual, ilCoding: bc =>
                    {
                        bc.Emitter.DeclareLocal(_property.Type);
                        bc.Emitter
                            .ldarg_0
                            .ldsfld(fieldBuilder.FieldBuilder)
                            .callvirt(GetValueMethod)
                            .Assert(op_Explicit != null, e1 => e1.call(op_Explicit),
                                e1 => e1.call(SGetValueMethod.MakeGenericMethod(_property.Type)))
                            .Assert(isEnum, e1 => e1.unbox_any(opType))
                            .stloc_0
                            .ldloc_0
                            .ret();
                    });

                var setMethod = propertyBuider.DefineSetMethod(calling: CallingDecoration.Virtual, ilCoding: bc =>
                    {
                        bc.Emitter
                            .ldarg_0
                            .ldsfld(fieldBuilder.FieldBuilder)
                            .ldarg_1
                            .Assert(isEnum, e1 => e1.box(_property.Type))
                            .Assert(op_Implicit != null, e1 => e1.call(op_Implicit),
                                e1 => e1.ldtoken(_property.Type).call(TypeGetTypeFromHandle).call(SNewValueMethod))
                            .callvirt(SetValueMethod)
                            .ret();
                    });

                setMethod.DefineParameter("value");

                if (validations != null && validations.TryGetValue(_property, out List<Expression<Func<Attribute>>> attribues))
                {
                    attribues.ForEach(s => propertyBuider.SetCustomAttribute(s));
                }

                fields.Add(fieldBuilder);
            }
        }

        private void DefineConstructors(IList<DynamicFieldBuilder> fields)
        {
            InnerBuilder.DefineConstructor(null);
            if (Properties != null)
            {
                InnerBuilder.DefineConstructor(null, ilCoding: bc =>
                    bc.Emitter.For(0, Properties.Count, (e, i) =>
                        RegisterProperty(fields[i], e, Properties[i]))
                        .ret(),
                    calling: CallingDecoration.Static);
            }
        }

        private EmitHelper RegisterProperty(DynamicFieldBuilder fieldBuilder, EmitHelper emiter, IProperty property)
        {
            var local = emiter.DeclareLocal(property.GetType());

            if (property is GeneralProperty)
            {
                emiter = InitGeneralPropertyInfo(emiter, local, property);
            }
            else if (property is RelationProperty)
            {
                emiter = InitRelationProperty(emiter, local, property);
            }
            else if (property is PropertyExtension ext)
            {
                return RegisterProperty(fieldBuilder, emiter, ext.Property);
            }
            else
            {
                emiter = emiter.ldnull;
            }

            return emiter.stsfld(fieldBuilder.FieldBuilder);
        }

        private EmitHelper InitGeneralPropertyInfo(EmitHelper emiter, LocalBuilder local, IProperty property)
        {
            var propertyType = property.GetType();
            var mapType = typeof(PropertyMapInfo);

            emiter = emiter
                .ldtoken(EntityType).call(TypeGetTypeFromHandle)
                .newobj(propertyType)
                .stloc(local)
                .ldloc(local)
                .ldstr(property.Name)
                .call(propertyType.GetProperty(nameof(GeneralProperty.Name)).GetSetMethod())
                .nop
                .ldloc(local)
                .ldtoken(property.Type)
                .call(TypeGetTypeFromHandle)
                .call(propertyType.GetProperty(nameof(GeneralProperty.Type)).GetSetMethod())
                .nop
                .ldloc(local)
                .newobj(typeof(PropertyMapInfo))
                .dup
                .ldstr(property.Info.FieldName)
                .call(mapType.GetProperty(nameof(PropertyMapInfo.FieldName)).GetSetMethod())
                .nop;

            if (property.Info.IsPrimaryKey)
            {
                emiter = emiter.dup.ldc_i4_1.call(mapType.GetProperty(nameof(PropertyMapInfo.IsPrimaryKey)).GetSetMethod()).nop;
            }

            if (property.Info.GenerateType != IdentityGenerateType.None)
            {
                emiter = emiter.dup.ldc_i4_((int)property.Info.GenerateType).call(mapType.GetProperty(nameof(PropertyMapInfo.GenerateType)).GetSetMethod()).nop;
            }

            if (property.Info.DataType != null)
            {
                emiter = emiter.dup.ldc_i4_((int)property.Info.DataType).newobj(typeof(DbType?), typeof(DbType)).call(mapType.GetProperty(nameof(PropertyMapInfo.DataType)).GetSetMethod()).nop;
            }

            if (!string.IsNullOrEmpty(property.Info.Description))
            {
                emiter = emiter.dup.ldstr(property.Info.Description).call(mapType.GetProperty(nameof(PropertyMapInfo.Description)).GetSetMethod()).nop;
            }

            if (property.Info.IsDeletedKey)
            {
                emiter = emiter.dup.ldc_i4_1.call(mapType.GetProperty(nameof(PropertyMapInfo.IsDeletedKey)).GetSetMethod()).nop;
            }

            if (property.Info.IsNullable)
            {
                emiter = emiter.dup.ldc_i4_1.call(mapType.GetProperty(nameof(PropertyMapInfo.IsNullable)).GetSetMethod()).nop;
            }

            if (property.Info.Precision != null)
            {
                emiter = emiter.dup.ldc_i4_((int)property.Info.Precision).newobj(typeof(int?), typeof(int)).call(mapType.GetProperty(nameof(PropertyMapInfo.Precision)).GetSetMethod()).nop;
            }

            if (property.Info.Scale != null)
            {
                emiter = emiter.dup.ldc_i4_((int)property.Info.Scale).newobj(typeof(int?), typeof(int)).call(mapType.GetProperty(nameof(PropertyMapInfo.Scale)).GetSetMethod()).nop;
            }

            if (property.Info.Length != null)
            {
                emiter = emiter.dup.ldc_i4_((int)property.Info.Length).newobj(typeof(int?), typeof(int)).call(mapType.GetProperty(nameof(PropertyMapInfo.Length)).GetSetMethod()).nop;
            }

            if (!PropertyValue.IsEmpty(property.Info.DefaultValue))
            {
                //emiter = emiter.dup.ldc_i4_((int)property.Info.Length).newobj(typeof(int?), typeof(int)).call(mapType.GetProperty(nameof(PropertyMapInfo.Length)).GetSetMethod()).nop;
            }

            return emiter.call(propertyType.GetProperty(nameof(GeneralProperty.Info)).GetSetMethod())
                .ldloc(local)
                .call(RegisterMethod);
        }

        private EmitHelper InitRelationProperty(EmitHelper emiter, LocalBuilder local, IProperty property)
        {
            var propertyType = property.GetType();

            return emiter
                .ldtoken(EntityType).call(TypeGetTypeFromHandle)
                .newobj(propertyType)
                .stloc(local)
                .ldloc(local)
                .ldstr(property.Name)
                .call(propertyType.GetProperty(nameof(GeneralProperty.Name)).GetSetMethod())
                .nop
                .ldloc(local)
                .ldtoken(property.Type)
                .call(TypeGetTypeFromHandle)
                .call(propertyType.GetProperty(nameof(GeneralProperty.Type)).GetSetMethod())
                .nop
                .ldloc(local)
                .ldtoken(property.Type)
                .call(TypeGetTypeFromHandle)
                .call(propertyType.GetProperty(nameof(RelationProperty.RelationalType)).GetSetMethod())
                .nop
                .ldloc(local)
                .ldsfld(typeof(RelationOptions).GetField(nameof(RelationOptions.Default), BindingFlags.Public | BindingFlags.Static))
                .call(propertyType.GetProperty(nameof(RelationProperty.Options)).GetSetMethod())
                .nop
                .ldloc(local).call(RegisterMethod);
        }

        /// <summary>
        /// 创建一个新对象并初始化的表达式。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private Expression MakeMemberInitExpression(object obj)
        {
            var newExp = Expression.New(obj.GetType());
            var binds = new List<MemberBinding>();

            obj.Compare((pro, value) =>
                {
                    if (value is IProperty)
                    {
                        return;
                    }

                    if (value is PropertyValue propertyValue)
                    {
                        binds.Add(Expression.Bind(pro, Expression.Convert(Expression.Constant(propertyValue.GetValue()), typeof(PropertyValue))));
                    }
                    else if (pro.PropertyType.IsValueType || pro.PropertyType == typeof(string))
                    {
                        binds.Add(pro.PropertyType.IsNullableType()
                            ? Expression.Bind(pro, Expression.Convert(Expression.Constant(value), pro.PropertyType))
                            : Expression.Bind(pro, Expression.Constant(value)));
                    }
                    else if (pro.PropertyType == typeof(Type))
                    {
                        binds.Add(Expression.Bind(pro, Expression.Constant(value, typeof(Type))));
                    }
                    else
                    {
                        binds.Add(Expression.Bind(pro, MakeMemberInitExpression(value)));
                    }
                });

            return Expression.MemberInit(newExp, binds);
        }

        /// <summary>
        /// 根据一个 <see cref="IProperty"/> 创建一个 <see cref="LambdaExpression"/>。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private LambdaExpression MakeLambdaExpression(IProperty property)
        {
            return Expression.Lambda<Func<IProperty>>(MakeMemberInitExpression(property));
        }
    }
}