//#define OUT //用于在调试时生成程序集
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Caching;
using Fireasy.Common.Emit;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

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

        /// <summary>
        /// 创建一个代理类型。
        /// </summary>
        /// <param name="type">要注入AOP的类型。</param>
        /// <param name="option"></param>
        /// <returns>代理类。</returns>
        public static Type BuildTypeCached(Type type, InterceptBuildOption option = null)
        {
            var cacheMgr = MemoryCacheManager.Instance;
            return cacheMgr.TryGet(string.Concat("Aspect_", type.FullName), () => BuildType(type, option), () => NeverExpired.Instance);
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

            var interceptMethod = DefineInterceptMethod(builder);
            var globalIntercepts = type.GetCustomAttributes<InterceptAttribute>(true).ToList();

            DefineConstructors(builder, globalIntercepts);

            FindAndInjectMethods(builder, members, globalIntercepts, interceptMethod);
            FindAndInjectProperties(builder, members, globalIntercepts, interceptMethod);

            try
            {
#if OUT
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

        private static bool IsCanIntercepte(MemberInfo member)
        {
            if (member is MethodInfo && (member as MethodInfo).IsVirtual)
            {
                var method = member as MethodInfo;
                return method.IsVirtual && !method.IsFinal;
            }

            if (member is PropertyInfo)
            {
                var property = member as PropertyInfo;
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
        /// <param name="globalIntercepts"></param>
        /// <param name="builder"></param>
        private static void DefineConstructors(DynamicTypeBuilder builder, IList<InterceptAttribute> globalIntercepts)
        {
            var constructors = builder.BaseType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            foreach (var conInfo in constructors)
            {
                var parameters = conInfo.GetParameters();

                //定义构造函数，在方法体内调用父类的同一方法
                //如 base.dosomething(a, b, c);
                var cb = builder.DefineConstructor(parameters.Select(s => s.ParameterType).ToArray(), ilCoding:
                        bc => bc.Emitter.ldarg_0.For(0, parameters.Length, (e, i) => e.ldarg(i + 1)).call(conInfo).ret()
                    );

                //定义参数
                foreach (var par in parameters)
                {
                    cb.DefineParameter(par.Name, par.DefaultValue != DBNull.Value, par.DefaultValue);
                }
            }
        }

        /// <summary>
        /// 在代理类中定义一个调用拦截器的方法。
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
                for (int i = 0; i < interceptors.Count; i++)
                {
                    interceptors[i].Intercept(callInfo);
                }
            }
            */
            #endregion
            var callMethod = builder.DefineMethod(
                string.Concat(AOP_PREFIX, "Intercept"), 
                null, 
                new[] { typeof(List<IInterceptor>), typeof(InterceptCallInfo), typeof(InterceptType) }, 
                VisualDecoration.Private, ilCoding: ctx =>
                    {
                        var l1 = ctx.Emitter.DefineLabel();
                        var l2 = ctx.Emitter.DefineLabel();
                        ctx.Emitter.DeclareLocal(typeof(int));
                        ctx.Emitter
                            .ldarg_2
                            .ldarg_3
                            .call(InterceptCache.CallInfoSetInterceptType)
                            .ldc_i4_0
                            .stloc_0
                            .br_s(l2)
                            .MarkLabel(l1)
                            .ldarg_1
                            .ldloc_0
                            .call(InterceptCache.InterceptorsGetItem)
                            .ldarg_2
                            .call(InterceptCache.InterceptorIntercept)
                            .ldloc_0
                            .ldc_i4_1
                            .add
                            .stloc_0
                            .MarkLabel(l2)
                            .ldloc_0
                            .ldarg_1
                            .call(InterceptCache.InterceptorsGetCount)
                            .blt_s(l1)
                            .ret();
                    });

            callMethod.DefineParameter("interceptors");
            callMethod.DefineParameter("callInfo");
            callMethod.DefineParameter("interceptType");
            
            return callMethod;
        }

        /// <summary>
        /// 为每一个拦截的方法定义一个初始化上下文的方法。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="methodName">所要拦截的方法。</param>
        /// <returns></returns>
        private static DynamicMethodBuilder DefineInitializeMethod(DynamicTypeBuilder builder, string methodName)
        {
            #region 方法原型
            /*
            private void <Aspect>_MethodName_Initialize(List<IInterceptor> interceptors)
            {
                if (!this.<Aspect>_TestContext_Initialized)
                {
                    List<InterceptAttribute> list = new List<InterceptAttribute>();
                    InterceptAttribute item = new CustomInterceptAttribute {
                        Name = "测试",
                        Version = 12,
                        ValueType = typeof(TestClass),
                        InterceptorType = typeof(TestInterceptor4),
                        AllowThrowException = true
                    };
                    list.Add(item);
                    for (int i = 0; i < interceptors.Count; i++)
                    {
                        InterceptContext context = new InterceptContext {
                            Attribute = list[i],
                            Target = this
                        };
                        interceptors[i].Initialize(context);
                    }
                    this.<Aspect>_TestContext_Initialized = true;
                }
            }
             */
            #endregion
            var fieldBuilder = builder.TypeBuilder.DefineField(
                string.Concat(AOP_PREFIX, methodName, "_Initialized"), 
                typeof(bool), 
                FieldAttributes.Private);

            var callMethod = builder.DefineMethod(
                string.Concat(AOP_PREFIX, methodName, "_Initialize"), 
                null, 
                new[] { typeof(List<IInterceptor>), typeof(InterceptCallInfo) }, 
                VisualDecoration.Private, 
                ilCoding: ctx =>
                    {
                        var l1 = ctx.Emitter.DefineLabel();
                        var l2 = ctx.Emitter.DefineLabel();
                        var l3 = ctx.Emitter.DefineLabel();
                        ctx.Emitter.DeclareLocal(typeof(int));
                        ctx.Emitter.DeclareLocal(typeof(object[]));
                        ctx.Emitter.DeclareLocal(typeof(InterceptContext));
                        ctx.Emitter.DeclareLocal(typeof(InterceptAttribute));
                        ctx.Emitter
                            .ldarg_0
                            .ldfld(fieldBuilder)
                            .ldc_bool(true)
                            .beq(l3)
                            .ldarg_2
                            .call(InterceptCache.MethodAllGetAttributes)
                            .stloc_1
                            .ldc_i4_0
                            .stloc_0
                            .br_s(l2)
                            .MarkLabel(l1)
                            .newobj(typeof(InterceptContext))
                            .stloc_2
                            .ldloc_2
                            .ldloc_1
                            .ldloc_0
                            .ldelem_ref
                            .castType(typeof(InterceptAttribute))
                            .callvirt(InterceptCache.InterceptContextSetAttribute)
                            .ldloc_2
                            .ldarg_0
                            .callvirt(InterceptCache.CallInfoSetTarget)
                            .ldarg_1
                            .ldloc_0
                            .call(InterceptCache.InterceptorsGetItem)
                            .ldloc_2
                            .callvirt(InterceptCache.InterceptorInitialize)
                            .ldloc_0
                            .ldc_i4_1
                            .add
                            .stloc_0
                            .MarkLabel(l2)
                            .ldloc_0
                            .ldarg_1
                            .call(InterceptCache.InterceptorsGetCount)
                            .blt_s(l1)
                            .ldarg_0
                            .ldc_bool(true)
                            .stfld(fieldBuilder)
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
        /// <param name="builder"></param>
        /// <param name="members"></param>
        /// <param name="globalIntercepts"></param>
        /// <param name="interceptMethod">前面定义的拦截方法体。</param>
        private static void FindAndInjectMethods(DynamicTypeBuilder builder, IList<MemberInfo> members, IList<InterceptAttribute> globalIntercepts, DynamicMethodBuilder interceptMethod)
        {
            foreach (MethodInfo method in members.Where(s => s is MethodInfo))
            {
                InjectMethod(builder, globalIntercepts, interceptMethod, method);
            }
        }

        /// <summary>
        /// 查找并向属性体内注入代码。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="members"></param>
        /// <param name="globalIntercepts"></param>
        /// <param name="interceptMethod">前面定义的拦截方法体。</param>
        private static void FindAndInjectProperties(DynamicTypeBuilder builder, IList<MemberInfo> members, IList<InterceptAttribute> globalIntercepts, DynamicMethodBuilder interceptMethod)
        {
            var isInterface = builder.BaseType == typeof(object);

            foreach (PropertyInfo property in members.Where(s => s is PropertyInfo))
            {
                var propertyBuilder = builder.TypeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, Type.EmptyTypes);
                var field = isInterface ? builder.DefineField(string.Format("<{0}>__bkField", property.Name), property.PropertyType) : null;
                var method = property.GetGetMethod();

                if (method != null)
                {
                    propertyBuilder.SetGetMethod(InjectGetMethod(builder, field, globalIntercepts, interceptMethod, property, method).MethodBuilder);
                }

                method = property.GetSetMethod();
                if (method != null)
                {
                    propertyBuilder.SetSetMethod(InjectSetMethod(builder, field, globalIntercepts, interceptMethod, property, method).MethodBuilder);
                }
            }
        }

        /// <summary>
        /// 向方法体内注入代码。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="globalIntercepts"></param>
        /// <param name="interceptMethod">前面定义的拦截方法体。</param>
        /// <param name="method">所要注入的方法。</param>
        /// <returns></returns>
        private static DynamicMethodBuilder InjectMethod(DynamicTypeBuilder builder, IList<InterceptAttribute> globalIntercepts, DynamicMethodBuilder interceptMethod, MethodInfo method)
        {
            var attributes = method.GetCustomAttributes<InterceptAttribute>(true).Union(globalIntercepts);
            var isInterface = builder.BaseType == typeof(object);

            var initMethod = DefineInitializeMethod(builder, method.Name);

            #region 方法原型
            // var list = new List<InterceptAttribute>();
            // list.Add(new MyInterceptAttribute());
            // var info = new InterceptCallInfo();
            // info.Target = this;
            // info.Member = MethodInfo.GetCurrentMethod().GetBaseDefinition();
            // info.Arguments = new object[] {  };
            // try
            // {
            //     <Aspect>_方法_Initialize();
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
            var methodBuilder = builder.DefineMethod(
                method.Name, 
                method.ReturnType, 
                (from s in parameters select s.ParameterType).ToArray(),
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
                            ctx.Emitter.CallInitialize(initMethod)
                            .CallInterceptors(interceptMethod, InterceptType.BeforeMethodCall)
                            .ldloc(STACK_CALLINFO_INDEX)
                            .callvirt(InterceptCache.CallInfoGetCancel).brtrue_s(lblCancel)
                            .Assert(!isInterface, c => c.CallBaseMethod(method)
                                .Assert(isReturn, e => e.SetReturnValue(method.ReturnType)))
                            .MarkLabel(lblCancel)
                            .SetArguments(method)
                            .CallInterceptors(interceptMethod, InterceptType.AfterMethodCall)
                        .BeginCatchBlock(typeof(Exception))
                            .SetException()
                            .CallInterceptors(interceptMethod, InterceptType.Catching)
                            .Assert(AllowThrowException(attributes), e => e.ThrowException())
                        .BeginFinallyBlock()
                            .CallInterceptors(interceptMethod, InterceptType.Finally)
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
        /// <param name="builder"></param>
        /// <param name="field"></param>
        /// <param name="globalIntercepts"></param>
        /// <param name="interceptMethod"></param>
        /// <param name="property"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static DynamicMethodBuilder InjectGetMethod(DynamicTypeBuilder builder, DynamicFieldBuilder field, IList<InterceptAttribute> globalIntercepts, DynamicMethodBuilder interceptMethod, PropertyInfo property, MethodInfo method)
        {
            var attributes = property.GetCustomAttributes<InterceptAttribute>(true).Union(globalIntercepts);
            var isInterface = builder.BaseType == typeof(object);

            var initMethod = DefineInitializeMethod(builder, method.Name);

            var parameters = method.GetParameters();
            var methodBuilder = builder.DefineMethod(
                method.Name, 
                method.ReturnType, 
                (from s in parameters select s.ParameterType).ToArray(),
                ilCoding: ctx =>
                    {
                        var lblCancel = ctx.Emitter.DefineLabel();
                        var lblRet = ctx.Emitter.DefineLabel();
                        ctx.Emitter.DeclareLocal();
                        ctx.Emitter.DeclareLocal(property.PropertyType);
                        ctx.Emitter.InitInterceptors(attributes);
                        ctx.Emitter.InitLocal(property);
                        ctx.Emitter.BeginExceptionBlock();
                            ctx.Emitter.CallInitialize(initMethod)
                            .CallInterceptors(interceptMethod, InterceptType.BeforeGetValue)
                            .ldloc(STACK_CALLINFO_INDEX)
                            .callvirt(InterceptCache.CallInfoGetCancel).brtrue_s(lblCancel)
                            .Assert(isInterface, c => c.ldarg_0.ldfld(field.FieldBuilder), c => c.ldarg_0.call(method))
                            .SetReturnValue(method.ReturnType)
                            .MarkLabel(lblCancel)
                            .CallInterceptors(interceptMethod, InterceptType.AfterGetValue)
                        .BeginCatchBlock(typeof(Exception))
                            .SetException()
                            .CallInterceptors(interceptMethod, InterceptType.Catching)
                            .Assert(AllowThrowException(attributes), e => e.ThrowException())
                        .BeginFinallyBlock()
                            .CallInterceptors(interceptMethod, InterceptType.Finally)
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
        /// <param name="builder"></param>
        /// <param name="field"></param>
        /// <param name="globalIntercepts"></param>
        /// <param name="interceptMethod"></param>
        /// <param name="property"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static DynamicMethodBuilder InjectSetMethod(DynamicTypeBuilder builder, DynamicFieldBuilder field, IList<InterceptAttribute> globalIntercepts, DynamicMethodBuilder interceptMethod, PropertyInfo property, MethodInfo method)
        {
            var attributes = property.GetCustomAttributes<InterceptAttribute>(true).Union(globalIntercepts);
            var isInterface = builder.BaseType == typeof(object);

            var initMethod = DefineInitializeMethod(builder, method.Name);
            
            var parameters = method.GetParameters();
            var methodBuilder = builder.DefineMethod(
                method.Name, 
                method.ReturnType, 
                (from s in parameters select s.ParameterType).ToArray(),
                ilCoding: ctx =>
                    {
                        var lblCancel = ctx.Emitter.DefineLabel();
                        ctx.Emitter.DeclareLocal();
                        ctx.Emitter.InitInterceptors(attributes);
                        ctx.Emitter.InitLocal(property, method);
                        ctx.Emitter.BeginExceptionBlock();
                            ctx.Emitter.CallInitialize(initMethod)
                            .CallInterceptors(interceptMethod, InterceptType.BeforeSetValue)
                            .ldloc(STACK_CALLINFO_INDEX)
                            .callvirt(InterceptCache.CallInfoGetCancel).brtrue_s(lblCancel)
                            .Assert(isInterface, c => c.ldarg_0.ldarg_1.stfld(field.FieldBuilder), c => c.ldarg_0.GetArguments(property.PropertyType, 0).call(method))
                            .MarkLabel(lblCancel)
                            .CallInterceptors(interceptMethod, InterceptType.AfterSetValue)
                        .BeginCatchBlock(typeof(Exception))
                            .SetException()
                            .CallInterceptors(interceptMethod, InterceptType.Catching)
                            .Assert(AllowThrowException(attributes), e => e.ThrowException())
                        .BeginFinallyBlock()
                            .CallInterceptors(interceptMethod, InterceptType.Finally)
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
                    (e, t, i) =>
                        e.ldloc(STACK_INTERCEPTOR_LIST_INDEX)
                        .newobj(t.InterceptorType)
                        .callvirt(InterceptCache.InterceptorsAdd))
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
                .callvirt(InterceptCache.CallInfoSetTarget);
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
                .callvirt(InterceptCache.CallInfoSetException);
        }

        /// <summary>
        /// 设置返回值信息。
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        private static EmitHelper SetReturnValue(this EmitHelper emitter, Type returnType)
        {
            return emitter
                .stloc(STACK_RETURNVALUE_INDEX)
                .ldloc(STACK_CALLINFO_INDEX)
                .ldloc(STACK_RETURNVALUE_INDEX)
                .box(returnType)
                .call(InterceptCache.CallInfoSetReturnValue);
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
            return emitter
                .ldloc(STACK_CALLINFO_INDEX)
                .callvirt(InterceptCache.CallInfoGetReturnValue)
                .Assert(returnType.IsValueType, e1 =>
                    e1.brtrue_s(lbRetValNotNull)
                        .ldtoken(returnType)
                        .call(InterceptCache.TypeGetTypeFromHandle)
                        .call(InterceptCache.MethodGetDefaultValue)
                        .Assert(returnType.IsValueType, e => e.unbox_any(returnType))
                        .ret()
                        .MarkLabel(lbRetValNotNull)
                        .ldloc(STACK_CALLINFO_INDEX)
                        .callvirt(InterceptCache.CallInfoGetReturnValue)
                        .Assert(returnType.IsValueType, e => e.unbox_any(returnType))
                        .ret(),
                    e1 =>
                        e1.ret()
                    );
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
                    (e, t, i) =>
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
                .callvirt(InterceptCache.CallInfoSetArguments);
        }

        private static EmitHelper GetArguments(this EmitHelper emitter, Type argumentType, int index)
        {
            return emitter.ldloc(2)
                .callvirt(InterceptCache.CallInfoGetArguments)
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
                emitter.call(InterceptCache.MethodGetCurrent)
                    .castclass(typeof(MethodInfo))
                    .callvirt(InterceptCache.MethodGetBaseDefinition);
            }
            else if (member is PropertyInfo)
            {
                //使用 Type.GetProperty(propertyName) 获得属性
                emitter.ldtoken(member.DeclaringType)
                    .call(InterceptCache.TypeGetTypeFromHandle)
                    .ldstr(member.Name)
                    .call(InterceptCache.TypeGetProperty);
            }

            return emitter
                .callvirt(InterceptCache.CallInfoSetMember);
        }

        private static EmitHelper SetCurrentDefinedType(this EmitHelper emitter, MemberInfo member)
        {
            return emitter
                .ldloc(STACK_CALLINFO_INDEX)
                .ldtoken(member.DeclaringType)
                .call(InterceptCache.TypeGetTypeFromHandle)
                .callvirt(InterceptCache.CallInfoSetDefinedType);
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
    }
}
