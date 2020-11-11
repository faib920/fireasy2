//#define OUT //用于在调试时生成程序集
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
using Fireasy.Common.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fireasy.Common.Aop
{
    /// <summary>
    /// 代理类型的构造器，向属性或方法内注入拦截器的执行过程。
    /// </summary>
    public static class InterceptBuilder
    {
        /// <summary>
        /// 成员的前缀
        /// </summary>
        private const string AOP_PREFIX = "<Aspect>_";

        //所定义的变量所在的栈位置
        private const short STACK_EXCEPTION_INDEX = 0;
        private const short STACK_INTERCEPTOR_LIST_INDEX = 1;
        private const short STACK_CALLINFO_INDEX = 2;
        private const short STACK_ARGUMENT_INDEX = 3;
        private const short STACK_RETURNVALUE_INDEX = 4;

        private class MethodCache
        {
            internal protected static MethodInfo TypeGetMethod = typeof(Type).GetMethod(nameof(Type.GetMethod), new[] { typeof(string) });
            internal protected static MethodInfo MethodGetCurrent = typeof(MethodBase).GetMethod(nameof(MethodBase.GetCurrentMethod), BindingFlags.Public | BindingFlags.Static);
            internal protected static MethodInfo MethodGetBaseDefinition = typeof(MethodInfo).GetMethod(nameof(MethodInfo.GetBaseDefinition));
            internal protected static MethodInfo TypeGetProperty = typeof(Type).GetMethod(nameof(Type.GetProperty), new[] { typeof(string) });
            internal protected static MethodInfo MemberGetCustomAttributes = typeof(MemberInfo).GetMethod(nameof(MemberInfo.GetCustomAttributes), new[] { typeof(Type), typeof(bool) });
            internal protected static MethodInfo TypeGetTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Public | BindingFlags.Static);
            internal protected static MethodInfo CallInfoGetMember = typeof(InterceptCallInfo).GetMethod($"get_{nameof(InterceptCallInfo.Member)}");
            internal protected static MethodInfo CallInfoSetMember = typeof(InterceptCallInfo).GetMethod($"set_{nameof(InterceptCallInfo.Member)}");
            internal protected static MethodInfo CallInfoSetTarget = typeof(InterceptCallInfo).GetMethod($"set_" + nameof(InterceptCallInfo.Target));
            internal protected static MethodInfo CallInfoSetReturnType = typeof(InterceptCallInfo).GetMethod($"set_" + nameof(InterceptCallInfo.ReturnType));
            internal protected static MethodInfo CallInfoSetDefinedType = typeof(InterceptCallInfo).GetMethod($"set_{nameof(InterceptCallInfo.DefinedType)}");
            internal protected static MethodInfo CallInfoSetArguments = typeof(InterceptCallInfo).GetMethod($"set_{nameof(InterceptCallInfo.Arguments)}");
            internal protected static MethodInfo CallInfoGetArguments = typeof(InterceptCallInfo).GetMethod($"get_{nameof(InterceptCallInfo.Arguments)}");
            internal protected static MethodInfo CallInfoGetCancel = typeof(InterceptCallInfo).GetMethod($"get_{nameof(InterceptCallInfo.Cancel)}");
            internal protected static MethodInfo CallInfoSetReturnValue = typeof(InterceptCallInfo).GetMethod($"set_{nameof(InterceptCallInfo.ReturnValue)}");
            internal protected static MethodInfo CallInfoGetReturnValue = typeof(InterceptCallInfo).GetMethod($"get_{nameof(InterceptCallInfo.ReturnValue)}");
            internal protected static MethodInfo CallInfoSetException = typeof(InterceptCallInfo).GetMethod($"set_{nameof(InterceptCallInfo.Exception)}");
            internal protected static MethodInfo CallInfoSetInterceptType = typeof(InterceptCallInfo).GetMethod($"set_{nameof(InterceptCallInfo.InterceptType)}");
            internal protected static MethodInfo InterceptorsAdd = typeof(List<IInterceptor>).GetMethod(nameof(List<IInterceptor>.Add));
            internal protected static MethodInfo InterceptorsGetItem = typeof(List<IInterceptor>).GetMethod($"get_Item");
            internal protected static MethodInfo InterceptorsGetCount = typeof(List<IInterceptor>).GetMethod($"get_Count");
            internal protected static MethodInfo InterceptorIntercept = typeof(IInterceptor).GetMethod(nameof(IInterceptor.Intercept));
            internal protected static MethodInfo InterceptorInitialize = typeof(IInterceptor).GetMethod(nameof(IInterceptor.Initialize));
            internal protected static MethodInfo InterceptContextSetTarget = typeof(InterceptContext).GetMethod($"set_{nameof(InterceptContext.Target)}");
            internal protected static MethodInfo GetDefaultValue = typeof(ReflectionExtension).GetMethod(nameof(ReflectionExtension.GetDefaultValue));
            internal protected static MethodInfo TaskFromResult = typeof(Task).GetMethod(nameof(Task.FromResult));
            internal protected static MethodInfo MembersContains = typeof(List<MemberInfo>).GetMethod(nameof(List<MemberInfo>.Contains));
            internal protected static MethodInfo MembersAdd = typeof(List<MemberInfo>).GetMethod(nameof(List<MemberInfo>.Add));
            internal protected static MethodInfo TaskAsSync = typeof(TaskExtension).GetMethods().FirstOrDefault(s => s.Name == nameof(TaskExtension.AsSync) && !s.IsGenericMethod);
            internal protected static MethodInfo TaskAsSyncT = typeof(TaskExtension).GetMethods().FirstOrDefault(s => s.Name == nameof(TaskExtension.AsSync) && s.IsGenericMethod);
            internal protected static MethodInfo CompletedTask = typeof(TaskCompatible).GetProperty(nameof(TaskCompatible.CompletedTask)).GetMethod;
        }

        /// <summary>
        /// 创建一个代理类型，并将代理类放入缓存中。
        /// </summary>
        /// <param name="type">要注入AOP的类型。</param>
        /// <param name="option"></param>
        /// <returns>代理类。</returns>
        public static Type BuildTypeCached(Type type, InterceptBuildOption option = null)
        {
            return ReflectionCache.GetMember("Intercept", type, option, (t, o) => BuildType(t, o));
        }

        /// <summary>
        /// 创建一个代理类型。
        /// </summary>
        /// <param name="type">要注入AOP的类型。</param>
        /// <param name="option"></param>
        /// <returns>代理类。</returns>
        public static Type BuildType(Type type, InterceptBuildOption option = null)
        {
            Guard.ArgumentNull(type, nameof(type));
            if (type.IsSealed)
            {
                throw new AspectException(SR.GetString(SRKind.AopCanotCreateProxy_Sealed, type.FullName));
            }

            var members = GetInterceptMembers(type, type.IsDefined<InterceptAttribute>(true));

            //如果没有任何成员使用拦截器特性，则返回原来的类型
            if (members.IsNullOrEmpty())
            {
                return type;
            }

            var typeName = string.Format(option == null || string.IsNullOrEmpty(option.TypeNameFormatter)
                ? "<Aspect>_{0}" : option.TypeNameFormatter, type.Name);

            var assemblyBuilder = option != null && option.AssemblyBuilder != null ? option.AssemblyBuilder :
#if OUT
            new DynamicAssemblyBuilder(AOP_PREFIX + type.Assembly.FullName, "c:\\test\\aoptest.dll");
#else
            new DynamicAssemblyBuilder(AOP_PREFIX + type.Assembly.FullName);
#endif

            if (option != null && option.AssemblyInitializer != null)
            {
                option.AssemblyInitializer(assemblyBuilder);
            }

            var builder = assemblyBuilder.DefineType(typeName);
            if (type.IsInterface)
            {
                builder.ImplementInterface(type);
            }
            else
            {
                builder.BaseType = type;
            }

            builder.ImplementInterface(typeof(IAopImplement));

            if (option != null && option.TypeInitializer != null)
            {
                option.TypeInitializer(builder);
            }

            var context = new BuildInternalContext();
            context.Members = members;
            context.TypeBuilder = builder;
            context.InitializeMarkFieldBuilder = builder.DefineField($"{AOP_PREFIX}initMarks", typeof(List<MemberInfo>));
            context.InitializeMethodBuilder = DefineInitializeMethod(context);

            context.InterceptMethodBuilder = DefineInterceptMethod(builder);
            context.GlobalIntercepts = type.GetCustomAttributes<InterceptAttribute>(true).ToList();

            DefineConstructors(context);

            FindAndInjectMethods(context);
            FindAndInjectProperties(context);

            try
            {
#if OUT && !NETSTANDARD
                var proxyType = builder.CreateType();
                assemblyBuilder.Save();
                return proxyType;
#else
                return builder.CreateType();
#endif
            }
            catch (Exception exp)
            {
                throw new AspectException(SR.GetString(SRKind.AopCanotCreateProxy, type.FullName), exp);
            }
        }

        /// <summary>
        /// 获取所有使用 <see cref="InterceptAttribute"/> 特性修饰的成员。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="throughout"></param>
        /// <returns></returns>
        private static IList<MemberInfo> GetInterceptMembers(Type type, bool throughout)
        {
            var members = new List<MemberInfo>();
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (member.DeclaringType != type || Regex.IsMatch(member.Name, @"^set_|get_"))
                {
                    continue;
                }

                if ((throughout || member.IsDefined<InterceptAttribute>()) && IsCanIntercepte(member))
                {
                    members.Add(member);
                }
            }

            return members;
        }

        /// <summary>
        /// 判断成员是否能够被注入。
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        private static bool IsCanIntercepte(MemberInfo member)
        {
            if (member is MethodInfo method)
            {
                return method.IsVirtual && !method.IsFinal;
            }
            else if (member is PropertyInfo property)
            {
                var gm = property.GetGetMethod();
                var sm = property.GetSetMethod();
                if ((gm != null && gm.IsVirtual && !gm.IsFinal) || (sm != null && sm.IsVirtual && !sm.IsFinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 定义构造器重载。
        /// </summary>
        /// <param name="context"></param>
        private static void DefineConstructors(BuildInternalContext context)
        {
            var constructors = context.TypeBuilder.BaseType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            foreach (var conInfo in constructors)
            {
                var parameters = conInfo.GetParameters();

                //定义构造函数，在方法体内调用父类的同一方法
                //如 base.dosomething(a, b, c);
                var cb = context.TypeBuilder.DefineConstructor(parameters.Select(s => s.ParameterType).ToArray(), ilCoding:
                        bc => bc.Emitter.ldarg_0.For(0, parameters.Length, (e, i) => e.ldarg(i + 1)).call(conInfo)
                        .nop.nop.ldarg_0.newobj(typeof(List<MemberInfo>)).stfld(context.InitializeMarkFieldBuilder.FieldBuilder).ret()
                    );

                //定义参数
                foreach (var par in parameters)
                {
                    if (par.HasDefaultValue)
                    {
                        cb.DefineParameter(par.Name, par.DefaultValue);
                    }
                    else
                    {
                        cb.DefineParameter(par.Name);
                    }
                }
            }
        }

        /// <summary>
        /// 定义一个调用拦截器的方法。
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        private static DynamicMethodBuilder DefineInterceptMethod(DynamicTypeBuilder builder)
        {
            #region 方法原型
            /*
            private void <Aspect>_Intercept(List<IInterceptor> interceptors, InterceptCallInfo callInfo, InterceptType interceptType)
            {
                callInfo.InterceptType = interceptType;
                callInfo.Break = false;
                for (int i = 0; i < interceptors.Count; i++)
                {
                    if (callInfo.Break)
                    {
                        break;
                    }
                    interceptors[i].Intercept(callInfo);
                }
            }
            */
            #endregion
            var callMethod = builder.DefineMethod(
                $"{AOP_PREFIX}Intercept",
                null,
                new[] { typeof(List<IInterceptor>), typeof(InterceptCallInfo), typeof(InterceptType) },
                VisualDecoration.Private, ilCoding: ctx =>
                    {
                        var l1 = ctx.Emitter.DefineLabel();
                        var l2 = ctx.Emitter.DefineLabel();
                        var l3 = ctx.Emitter.DefineLabel();
                        var l4 = ctx.Emitter.DefineLabel();
                        ctx.Emitter.DeclareLocal(typeof(int));
                        ctx.Emitter.DeclareLocal(typeof(bool));
                        ctx.Emitter.DeclareLocal(typeof(bool));
                        ctx.Emitter
                            .nop
                            .ldarg_2
                            .ldarg_3
                            .callvirt(MethodCache.CallInfoSetInterceptType)
                            .nop
                            .ldarg_2
                            .ldc_i4_0
                            .callvirt(typeof(InterceptCallInfo).GetProperty(nameof(InterceptCallInfo.Break)).SetMethod)
                            .nop
                            .ldc_i4_0
                            .stloc_0
                            .br_s(l2)
                            .MarkLabel(l1)
                            .nop
                            .ldarg_2
                            .callvirt(typeof(InterceptCallInfo).GetProperty(nameof(InterceptCallInfo.Break)).GetMethod)
                            .stloc_1
                            .ldloc_1
                            .brfalse_s(l4)
                            .nop
                            .br_s(l3)
                            .MarkLabel(l4)
                            .ldarg_1
                            .ldloc_0
                            .callvirt(MethodCache.InterceptorsGetItem)
                            .ldarg_2
                            .callvirt(MethodCache.InterceptorIntercept)
                            .nop
                            .nop
                            .ldloc_0
                            .ldc_i4_1
                            .add
                            .stloc_0
                            .MarkLabel(l2)
                            .ldloc_0
                            .ldarg_1
                            .callvirt(MethodCache.InterceptorsGetCount)
                            .clt
                            .stloc_2
                            .ldloc_2
                            .brtrue_s(l1)
                            .MarkLabel(l3)
                            .ret();
                    });

            callMethod.DefineParameter("interceptors");
            callMethod.DefineParameter("callInfo");
            callMethod.DefineParameter("interceptType");

            return callMethod;
        }

        /// <summary>
        /// 定义初始化方法。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static DynamicMethodBuilder DefineInitializeMethod(BuildInternalContext context)
        {
            #region 方法原型
            /*
            private void <Aspect>_Initialize(List<IInterceptor> interceptors, InterceptCallInfo callInfo)
            {
                if (!this.<Aspect>_initMarks.Contains(callInfo.Member))
                {
                    for (int i = 0; i < interceptors.Count; i++)
                    {
                        InterceptContext context = new InterceptContext(callInfo.Member, this);
                        interceptors[i].Initialize(context);
                    }
                    this.<Aspect>_initMarks.Add(callInfo.Member);
                }
            }
             */
            #endregion

            var callMethod = context.TypeBuilder.DefineMethod(
                $"{AOP_PREFIX}Initialize",
                null,
                new[] { typeof(List<IInterceptor>), typeof(InterceptCallInfo) },
                VisualDecoration.Private,
                ilCoding: ctx =>
                    {
                        var l1 = ctx.Emitter.DefineLabel();
                        var l2 = ctx.Emitter.DefineLabel();
                        var l3 = ctx.Emitter.DefineLabel();
                        ctx.Emitter.DeclareLocal(typeof(int));
                        ctx.Emitter.DeclareLocal(typeof(InterceptContext));
                        ctx.Emitter
                            .ldarg_0
                            .ldfld(context.InitializeMarkFieldBuilder)
                            .ldarg_2
                            .callvirt(MethodCache.CallInfoGetMember)
                            .callvirt(MethodCache.MembersContains)
                            .ldc_bool(true)
                            .beq(l3)
                            .nop
                            .ldarg_2
                            .callvirt(MethodCache.CallInfoGetMember)
                            .ldarg_0
                            .newobj(typeof(InterceptContext), typeof(MemberInfo), typeof(object))
                            .stloc_1
                            .ldc_i4_0
                            .stloc_0
                            .br_s(l2)
                            .MarkLabel(l1)
                            .ldarg_1
                            .ldloc_0
                            .call(MethodCache.InterceptorsGetItem)
                            .ldloc_1
                            .callvirt(MethodCache.InterceptorInitialize)
                            .ldloc_0
                            .ldc_i4_1
                            .add
                            .stloc_0
                            .MarkLabel(l2)
                            .ldloc_0
                            .ldarg_1
                            .call(MethodCache.InterceptorsGetCount)
                            .blt_s(l1)
                            .ldarg_0
                            .ldfld(context.InitializeMarkFieldBuilder)
                            .ldarg_2
                            .callvirt(MethodCache.CallInfoGetMember)
                            .callvirt(MethodCache.MembersAdd)
                            .MarkLabel(l3)
                            .ret();
                    });

            callMethod.DefineParameter("interceptors");
            callMethod.DefineParameter("callInfo");
            return callMethod;
        }

        /// <summary>
        /// 查找并向方法体内注入代码。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="interceptMethod">前面定义的拦截方法体。</param>
        private static void FindAndInjectMethods(BuildInternalContext context)
        {
            foreach (MethodInfo method in context.Members.Where(s => s is MethodInfo))
            {
                InjectMethod(context, method);
            }
        }

        /// <summary>
        /// 查找并向属性体内注入代码。
        /// </summary>
        /// <param name="context"></param>
        private static void FindAndInjectProperties(BuildInternalContext context)
        {
            var isInterface = context.TypeBuilder.BaseType == typeof(object);

            foreach (PropertyInfo property in context.Members.Where(s => s is PropertyInfo))
            {
                var propertyBuilder = context.TypeBuilder.TypeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, Type.EmptyTypes);
                var field = isInterface ? context.TypeBuilder.DefineField($"<{property.Name}>__bkField", property.PropertyType) : null;
                var method = property.GetGetMethod();

                if (method != null)
                {
                    propertyBuilder.SetGetMethod(InjectGetMethod(context, field, property).MethodBuilder);
                }

                method = property.GetSetMethod();
                if (method != null)
                {
                    propertyBuilder.SetSetMethod(InjectSetMethod(context, field, property).MethodBuilder);
                }
            }
        }

        /// <summary>
        /// 向方法体内注入代码。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="method">所要注入的方法。</param>
        /// <returns></returns>
        private static DynamicMethodBuilder InjectMethod(BuildInternalContext context, MethodInfo method)
        {
            var attributes = method.GetCustomAttributes<InterceptAttribute>(true).Union(context.GlobalIntercepts);
            var isInterface = context.TypeBuilder.BaseType == typeof(object);

            #region 方法原型
            // var list = new List<InterceptAttribute>();
            // list.Add(new MyInterceptAttribute());
            // var info = new InterceptCallInfo();
            // info.Target = this;
            // info.Member = MethodInfo.GetCurrentMethod().GetBaseDefinition();
            // info.Arguments = new object[] {  };
            // try
            // {
            //     <Aspect>_Initialize(list, info);
            //     <Aspect>_Intercept(list, info, InterceptType.BeforeMethodCall);
            //     if (!info.Cancel)
            //     {
            //         base.方法(); //如果有返回  info.ReturnValue = base.方法();
            //     }
            //     info.Arguments = new object[] {  };
            //     <Aspect>_Intercept(list, info, InterceptType.AfterMethodCall);
            // }
            // catch (Exception exp)
            // {
            //     info.Exception = exp;
            //     <Aspect>_Intercept(list, info, InterceptType.Catching);
            //     //如果需要抛出 throw exp; 
            // }
            // finally
            // {
            //     <Aspect>_Intercept(list, info, InterceptType.Finally);
            // }
            // //如果返回
            // if (info.ReturnValue == null)
            // {
            //     return (bool)typeof(bool).GetDefaultValue();
            // }
            // else
            // {
            //     return (bool)info.ReturnValue;
            // };
            #endregion
            var parameters = method.GetParameters();
            var methodBuilder = context.TypeBuilder.DefineMethod(
                method.Name,
                method.ReturnType,
                parameters.Select(s => s.ParameterType).ToArray(),
                ilCoding: ctx =>
                    {
                        var isReturn = method.ReturnType != typeof(void);
                        var lblCancel = ctx.Emitter.DefineLabel();
                        var lblRet = ctx.Emitter.DefineLabel();
                        ctx.Emitter.DeclareLocal();
                        ctx.Emitter.Assert(isReturn, e => e.DeclareLocal(method.ReturnType));
                        ctx.Emitter.InitInterceptors(attributes);
                        ctx.Emitter.InitLocal(method, method);
                        ctx.Emitter.BeginExceptionBlock();
                        ctx.Emitter.CallInitialize(context.InitializeMethodBuilder)
                        .SetReturnType(method)
                        .CallInterceptors(context.InterceptMethodBuilder, InterceptType.BeforeMethodCall)
                        .ldloc(STACK_CALLINFO_INDEX)
                        .callvirt(MethodCache.CallInfoGetCancel).brtrue_s(lblCancel)
                        .Assert(!isInterface, c => c.CallBaseMethod(method)
                            .Assert(isReturn, e => e.SetReturnValue(method.ReturnType)))
                        .MarkLabel(lblCancel)
                        .CallInterceptors(context.InterceptMethodBuilder, InterceptType.AfterMethodCall)
                    .BeginCatchBlock(typeof(Exception))
                        .SetException()
                        .CallInterceptors(context.InterceptMethodBuilder, InterceptType.Catching)
                        .Assert(AllowThrowException(attributes), e => e.ThrowException())
                    .BeginFinallyBlock()
                        .CallInterceptors(context.InterceptMethodBuilder, InterceptType.Finally)
                    .EndExceptionBlock()
                    .Assert(isReturn, e1 => e1.GetReturnValue(method.ReturnType, lblRet), e1 => e1.ret());
                    });

            foreach (var par in parameters)
            {
                methodBuilder.DefineParameter(par.Name, par.IsOut, par.DefaultValue != DBNull.Value, par.DefaultValue);
            }

            return methodBuilder;
        }

        /// <summary>
        /// 向属性的 get 方法中注入代码。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fieldBuilder"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static DynamicMethodBuilder InjectGetMethod(BuildInternalContext context, DynamicFieldBuilder fieldBuilder, PropertyInfo property)
        {
            var attributes = property.GetCustomAttributes<InterceptAttribute>(true).Union(context.GlobalIntercepts);
            var isInterface = context.TypeBuilder.BaseType == typeof(object);

            var method = property.GetMethod;
            var parameters = method.GetParameters();
            var methodBuilder = context.TypeBuilder.DefineMethod(
                method.Name,
                method.ReturnType,
                parameters.Select(s => s.ParameterType).ToArray(),
                ilCoding: ctx =>
                    {
                        var lblCancel = ctx.Emitter.DefineLabel();
                        var lblRet = ctx.Emitter.DefineLabel();
                        ctx.Emitter.DeclareLocal();
                        ctx.Emitter.DeclareLocal(property.PropertyType);
                        ctx.Emitter.InitInterceptors(attributes);
                        ctx.Emitter.InitLocal(property, method);
                        ctx.Emitter.BeginExceptionBlock();
                        ctx.Emitter.CallInitialize(context.InitializeMethodBuilder)
                        .CallInterceptors(context.InterceptMethodBuilder, InterceptType.BeforeGetValue)
                        .ldloc(STACK_CALLINFO_INDEX)
                        .callvirt(MethodCache.CallInfoGetCancel).brtrue_s(lblCancel)
                        .Assert(isInterface, c => c.ldarg_0.ldfld(fieldBuilder.FieldBuilder), c => c.ldarg_0.call(method))
                        .SetReturnValue(method.ReturnType)
                        .MarkLabel(lblCancel)
                        .CallInterceptors(context.InterceptMethodBuilder, InterceptType.AfterGetValue)
                    .BeginCatchBlock(typeof(Exception))
                        .SetException()
                        .CallInterceptors(context.InterceptMethodBuilder, InterceptType.Catching)
                        .Assert(AllowThrowException(attributes), e => e.ThrowException())
                    .BeginFinallyBlock()
                        .CallInterceptors(context.InterceptMethodBuilder, InterceptType.Finally)
                    .EndExceptionBlock()
                    .GetReturnValue(property.PropertyType, lblRet);
                    });

            foreach (var par in parameters)
            {
                methodBuilder.DefineParameter(par.Name);
            }

            return methodBuilder;
        }

        /// <summary>
        /// 向属性的 set 方法中注入代码。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fieldBuilder"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static DynamicMethodBuilder InjectSetMethod(BuildInternalContext context, DynamicFieldBuilder fieldBuilder, PropertyInfo property)
        {
            var attributes = property.GetCustomAttributes<InterceptAttribute>(true).Union(context.GlobalIntercepts);
            var isInterface = context.TypeBuilder.BaseType == typeof(object);

            var method = property.SetMethod;

            var parameters = method.GetParameters();
            var methodBuilder = context.TypeBuilder.DefineMethod(
                method.Name,
                method.ReturnType,
                parameters.Select(s => s.ParameterType).ToArray(),
                ilCoding: ctx =>
                    {
                        var lblCancel = ctx.Emitter.DefineLabel();
                        ctx.Emitter.DeclareLocal();
                        ctx.Emitter.InitInterceptors(attributes);
                        ctx.Emitter.InitLocal(property, method);
                        ctx.Emitter.BeginExceptionBlock();
                        ctx.Emitter.CallInitialize(context.InitializeMethodBuilder)
                        .CallInterceptors(context.InterceptMethodBuilder, InterceptType.BeforeSetValue)
                        .ldloc(STACK_CALLINFO_INDEX)
                        .callvirt(MethodCache.CallInfoGetCancel).brtrue_s(lblCancel)
                        .Assert(isInterface, c => c.ldarg_0.ldarg_1.stfld(fieldBuilder.FieldBuilder), c => c.ldarg_0.GetArguments(property.PropertyType, 0).call(method))
                        .MarkLabel(lblCancel)
                        .CallInterceptors(context.InterceptMethodBuilder, InterceptType.AfterSetValue)
                    .BeginCatchBlock(typeof(Exception))
                        .SetException()
                        .CallInterceptors(context.InterceptMethodBuilder, InterceptType.Catching)
                        .Assert(AllowThrowException(attributes), e => e.ThrowException())
                    .BeginFinallyBlock()
                        .CallInterceptors(context.InterceptMethodBuilder, InterceptType.Finally)
                    .EndExceptionBlock()
                    .ret();
                    });

            foreach (var par in parameters)
            {
                methodBuilder.DefineParameter(par.Name);
            }

            return methodBuilder;
        }

        /// <summary>
        /// 定义变量。
        /// </summary>
        /// <param name="emitter"></param>
        /// <returns></returns>
        private static EmitHelper DeclareLocal(this EmitHelper emitter)
        {
            emitter.DeclareLocal(typeof(Exception));
            emitter.DeclareLocal(typeof(List<IInterceptor>));
            emitter.DeclareLocal(typeof(InterceptCallInfo));
            emitter.DeclareLocal(typeof(object[]));
            return emitter;
        }

        /// <summary>
        /// 抛出异常。
        /// </summary>
        /// <param name="emitter"></param>
        /// <returns></returns>
        private static EmitHelper ThrowException(this EmitHelper emitter)
        {
            return emitter
                .ldloc(STACK_EXCEPTION_INDEX)
                .@throw
                .end();
        }

        /// <summary>
        /// 初始化拦截器。
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private static EmitHelper InitInterceptors(this EmitHelper emitter, IEnumerable<InterceptAttribute> attributes)
        {
            var type = typeof(List<IInterceptor>);
            return emitter
                .newobj(type)
                .stloc(STACK_INTERCEPTOR_LIST_INDEX)
                .Each(
                    attributes,
                    (e, a, i) =>
                        e.ldloc(STACK_INTERCEPTOR_LIST_INDEX)
                        .newobj(a.InterceptorType)
                        .callvirt(MethodCache.InterceptorsAdd))
                .newobj(typeof(InterceptCallInfo))
                .stloc(STACK_CALLINFO_INDEX)
                .end();
        }

        /// <summary>
        /// 调用父类的方法。
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static EmitHelper CallBaseMethod(this EmitHelper emitter, MethodInfo method)
        {
            var c = method.GetParameters().Length;
            return emitter
                .ldarg_0
                .For(0, c, (e, i) => e.ldarg(i + 1))
                .call(method);
        }

        /// <summary>
        /// 调用拦截器。
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="interceptMethod"></param>
        /// <param name="interceptType"></param>
        /// <returns></returns>
        private static EmitHelper CallInterceptors(this EmitHelper emitter, DynamicMethodBuilder interceptMethod, InterceptType interceptType)
        {
            return emitter
                .ldarg_0
                .ldloc(STACK_INTERCEPTOR_LIST_INDEX)
                .ldloc(STACK_CALLINFO_INDEX)
                .ldc_i4((int)interceptType)
                .call(interceptMethod.MethodBuilder);
        }

        /// <summary>
        /// 调用初始化拦截器的方法。
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="initMethod"></param>
        /// <returns></returns>
        private static EmitHelper CallInitialize(this EmitHelper emitter, DynamicMethodBuilder initMethod)
        {
            return emitter
                .ldarg_0
                .ldloc(STACK_INTERCEPTOR_LIST_INDEX)
                .ldloc(STACK_CALLINFO_INDEX)
                .call(initMethod.MethodBuilder);
        }

        /// <summary>
        /// 初始化变量。
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="member"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static EmitHelper InitLocal(this EmitHelper emitter, MemberInfo member, MethodInfo method = null)
        {
            return emitter
                .SetCurrentTarget()
                .SetCurrentMember(member)
                .SetCurrentDefinedType(member)
                .Assert(method != null, e => e.SetArguments(method));
        }

        /// <summary>
        /// 设置当前实例。
        /// </summary>
        /// <param name="emitter"></param>
        /// <returns></returns>
        private static EmitHelper SetCurrentTarget(this EmitHelper emitter)
        {
            return emitter
                .ldloc(STACK_CALLINFO_INDEX)
                .ldarg_0
                .callvirt(MethodCache.CallInfoSetTarget);
        }

        /// <summary>
        /// 设置异常信息。
        /// </summary>
        /// <param name="emitter"></param>
        /// <returns></returns>
        private static EmitHelper SetException(this EmitHelper emitter)
        {
            return emitter
                .stloc(STACK_EXCEPTION_INDEX)
                .ldloc(STACK_CALLINFO_INDEX)
                .ldloc(STACK_EXCEPTION_INDEX)
                .callvirt(MethodCache.CallInfoSetException);
        }

        /// <summary>
        /// 设置返回值信息。
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        private static EmitHelper SetReturnValue(this EmitHelper emitter, Type returnType)
        {
            if (returnType == typeof(Task))
            {
                return emitter.call(MethodCache.TaskAsSync);
            }

            var isNotRet = returnType != typeof(Task) && returnType != typeof(void);
            var taskRetType = returnType.GetTaskReturnType();

            return emitter
                .stloc(STACK_RETURNVALUE_INDEX)
                .ldloc(STACK_CALLINFO_INDEX)
                .ldloc(STACK_RETURNVALUE_INDEX)
                .Assert(taskRetType != null, 
                    e => e.call(MethodCache.TaskAsSyncT.MakeGenericMethod(taskRetType))
                        .Assert(taskRetType.IsValueType, e1 => e1.box(taskRetType)), 
                    e => e.Assert(returnType.IsValueType, e1 => e1.box(returnType)))
                .Assert(isNotRet, e1 => e1.call(MethodCache.CallInfoSetReturnValue));
        }

        /// <summary>
        /// 设置返回值信息。
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="returnType"></param>
        /// <param name="lbRetValNotNull"></param>
        /// <returns></returns>
        private static EmitHelper GetReturnValue(this EmitHelper emitter, Type returnType, Label lbRetValNotNull)
        {
            var taskRetType = returnType.GetTaskReturnType();
            if (taskRetType != null)
            {
                returnType = taskRetType;
            }

            return emitter
                .Assert(returnType == typeof(Task), 
                e => e.call(MethodCache.CompletedTask).stloc(STACK_RETURNVALUE_INDEX).ldloc(STACK_RETURNVALUE_INDEX).ret(), 
                v =>
                {
                    v.ldloc(STACK_CALLINFO_INDEX)
                    .callvirt(MethodCache.CallInfoGetReturnValue)
                    .Assert(returnType.IsValueType, e1 =>
                        e1.brtrue_s(lbRetValNotNull)
                            .ldtoken(returnType)
                            .call(MethodCache.TypeGetTypeFromHandle)
                            .call(MethodCache.GetDefaultValue)
                            .Assert(returnType.IsValueType, e => e.unbox_any(returnType))
                            .Assert(taskRetType != null, e => e.call(MethodCache.TaskFromResult.MakeGenericMethod(taskRetType)))
                            .ret()
                            .MarkLabel(lbRetValNotNull)
                            .ldloc(STACK_CALLINFO_INDEX)
                            .callvirt(MethodCache.CallInfoGetReturnValue)
                            .Assert(returnType.IsValueType, e => e.unbox_any(returnType))
                            .Assert(taskRetType != null, e => e.call(MethodCache.TaskFromResult.MakeGenericMethod(taskRetType)))
                            .ret(),
                        e1 =>
                            e1
                            .Assert(returnType.IsValueType, e => e.unbox_any(returnType), e => e.isinst(returnType))
                            .Assert(taskRetType != null, e => e.call(MethodCache.TaskFromResult.MakeGenericMethod(taskRetType)))
                            .ret()
                        );
                });
        }

        /// <summary>
        /// 为 <see cref="InterceptCallInfo"/> 对象设置所注入方法的参数。
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static EmitHelper SetArguments(this EmitHelper emitter, MethodInfo method)
        {
            //方法不允许有ref和out类型的参数
            var parameters = method.GetParameters();
            return emitter
                .ldloc(STACK_CALLINFO_INDEX)
                .ldc_i4(parameters.Length)
                .newarr(typeof(object))
                .stloc(STACK_ARGUMENT_INDEX)
                .Each(
                    parameters,
                    (e, p, i) =>
                    {
                        var type = parameters[i].ParameterType;
                        var isByref = type.IsByRef;

                        //引用 &
                        if (isByref)
                        {
                            type = type.GetElementType();
                        }

                        e.ldloc(STACK_ARGUMENT_INDEX)
                            .ldc_i4(i)
                            .ldarg(i + 1)
                            .Assert(isByref,
                                e1 => e1.Assert(type.IsValueType, e2 => e2.ldobj(type).box(type), e2 => e2.ldind_ref.end()),
                                e1 => e1.Assert(type.IsValueType || type.IsGenericParameter, e2 => e2.box(type)))
                            .stelem_ref
                            .end();
                    })
                .ldloc(STACK_ARGUMENT_INDEX)
                .callvirt(MethodCache.CallInfoSetArguments);
        }

        public static EmitHelper SetReturnType(this EmitHelper emitter, MethodInfo method)
        {
            var returnType = method.ReturnType.GetTaskReturnType() ?? method.ReturnType;
            return emitter
                .ldloc(STACK_CALLINFO_INDEX)
                .ldtoken(returnType)
                .call(MethodCache.TypeGetTypeFromHandle)
                .callvirt(MethodCache.CallInfoSetReturnType);
        }

        private static EmitHelper GetArguments(this EmitHelper emitter, Type argumentType, int index)
        {
            return emitter.ldloc(2)
                .callvirt(MethodCache.CallInfoGetArguments)
                .ldc_i4(index)
                .ldelem_ref
                .Assert(argumentType.IsValueType, e => e.unbox_any(argumentType));
        }

        /// <summary>
        /// 为 <see cref="InterceptCallInfo"/> 对象设置 Member 属性。
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        private static EmitHelper SetCurrentMember(this EmitHelper emitter, MemberInfo member)
        {
            emitter.ldloc(STACK_CALLINFO_INDEX);

            if (member is MethodInfo)
            {
                //使用 MethodInfo.GetCurrentMethod().GetBaseDefinition() 方法获得父类里的定义
                emitter.call(MethodCache.MethodGetCurrent)
                    .castclass(typeof(MethodInfo))
                    .callvirt(MethodCache.MethodGetBaseDefinition);
            }
            else if (member is PropertyInfo)
            {
                //使用 Type.GetProperty(propertyName) 获得属性
                emitter.ldtoken(member.DeclaringType)
                    .call(MethodCache.TypeGetTypeFromHandle)
                    .ldstr(member.Name)
                    .call(MethodCache.TypeGetProperty);
            }

            return emitter
                .callvirt(MethodCache.CallInfoSetMember);
        }

        private static EmitHelper SetCurrentDefinedType(this EmitHelper emitter, MemberInfo member)
        {
            return emitter
                .ldloc(STACK_CALLINFO_INDEX)
                .ldtoken(member.DeclaringType)
                .call(MethodCache.TypeGetTypeFromHandle)
                .callvirt(MethodCache.CallInfoSetDefinedType);
        }

        /// <summary>
        /// 检查一组 Attribute 中是否有其中一个允许抛出异常。
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private static bool AllowThrowException(IEnumerable<InterceptAttribute> attributes)
        {
            return attributes.Any(attr => attr.AllowThrowException);
        }

        private class BuildInternalContext
        {
            public IList<MemberInfo> Members { get; set; }

            public IList<InterceptAttribute> GlobalIntercepts { get; set; }

            public DynamicTypeBuilder TypeBuilder { get; set; }

            public DynamicFieldBuilder InitializeMarkFieldBuilder { get; set; }

            public DynamicMethodBuilder InitializeMethodBuilder { get; set; }

            public DynamicMethodBuilder InterceptMethodBuilder { get; set; }
        }
    }
}
