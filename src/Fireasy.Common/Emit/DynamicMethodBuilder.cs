// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Fireasy.Common.Emit
{
    /// <summary>
    /// 用于创建一个动态的方法。
    /// </summary>
    public class DynamicMethodBuilder : DynamicBuilder
    {
        private MethodBuilder methodBuilder;
        private readonly MethodAttributes attributes;
        private readonly Action<BuildContext> action;
        private readonly List<string> parameters = new List<string>();
        private Type returnType;
        private Type[] parameterTypes;
        private string[] genericTypeNames;

        internal DynamicMethodBuilder(BuildContext context, string methodName, Type returnType = null, Type[] parameterTypes = null, VisualDecoration visual = VisualDecoration.Public, CallingDecoration calling = CallingDecoration.Standard, Action<BuildContext> ilCoding = null)
             : base(visual, calling)
        {
            Context = new BuildContext(context) { MethodBuilder = this };
            Name = methodName;
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
            this.action = ilCoding;
            attributes = GetMethodAttributes(methodName, parameterTypes, visual, calling);
            InitBuilder();
        }

        /// <summary>
        /// 定义一个参数。
        /// </summary>
        /// <param name="name">参数的名称。</param>
        /// <param name="isOut">是否为 out 类型的参数。</param>
        /// <param name="hasDefaultValue">是否指定缺省值。</param>
        /// <param name="defaultValue">缺省的参数值。</param>
        /// <returns></returns>
        public DynamicMethodBuilder DefineParameter(string name, bool isOut = false, bool hasDefaultValue = false, object defaultValue = null)
        {
            var attr = hasDefaultValue ? ParameterAttributes.HasDefault : ParameterAttributes.None;
            if (isOut)
            {
                attr |= ParameterAttributes.Out;
            }

            var parameter = MethodBuilder.DefineParameter(parameters.Count + 1, attr, name);
            if (hasDefaultValue)
            {
                parameter.SetConstant(defaultValue);
            }

            parameters.Add(name);
            return this;
        }

        /// <summary>
        /// 获取方法的名称。
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 获取或设置方法的返回类型。
        /// </summary>
        public Type ReturnType
        {
            get
            {
                return returnType;
            }

            set
            {
                returnType = value;
                if (methodBuilder != null)
                {
                    methodBuilder.SetReturnType(returnType);
                }
            }
        }

        /// <summary>
        /// 获取或设置方法的参数类型数组。
        /// </summary>
        public Type[] ParameterTypes
        {
            get
            {
                return parameterTypes;
            }

            set
            {
                parameterTypes = value;
                if (methodBuilder != null)
                {
                    methodBuilder.SetParameters(parameterTypes);
                }
            }
        }

        /// <summary>
        /// 获取或设置方法的泛型参数名称数组。
        /// </summary>
        public string[] GenericArguments
        {
            get
            {
                return genericTypeNames;
            }

            set
            {
                genericTypeNames = value;
                ProcessGenericMethod();
            }
        }

        /// <summary>
        /// 获取当前的 <see cref="MethodBuilder"/>。
        /// </summary>
        /// <returns></returns>
        public MethodBuilder MethodBuilder
        {
            get { return methodBuilder; }
        }

        /// <summary>
        /// 追加新的 MSIL 代码到构造器中。
        /// </summary>
        /// <param name="ilCoding"></param>
        /// <returns></returns>
        public DynamicMethodBuilder AppendCode(Action<EmitHelper> ilCoding)
        {
            if (ilCoding != null)
            {
                ilCoding(Context.Emitter);
            }

            return this;
        }

        /// <summary>
        /// 使用新的 MSIL 代码覆盖构造器中的现有代码。
        /// </summary>
        /// <param name="ilCoding"></param>
        /// <returns></returns>
        public DynamicMethodBuilder OverwriteCode(Action<EmitHelper> ilCoding)
        {
            var field = typeof(MethodBuilder).GetField("m_ilGenerator", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var cons = typeof(ILGenerator).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
                field.SetValue(methodBuilder, cons.Invoke(new [] { methodBuilder }));
                Context.Emitter = new EmitHelper(methodBuilder.GetILGenerator(), methodBuilder);
            }

            return AppendCode(ilCoding);
        }

        /// <summary>
        /// 设置一个 <see cref="CustomAttributeBuilder"/> 对象到当前实例关联的 <see cref="MethodBuilder"/> 对象。
        /// </summary>
        /// <param name="customBuilder">一个 <see cref="CustomAttributeBuilder"/> 对象。</param>
        protected override void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            MethodBuilder.SetCustomAttribute(customBuilder);
        }

        private MethodInfo FindMethod(string methodName, IEnumerable<Type> parameterTypes)
        {
            MethodInfo method = null;
            if (Context.TypeBuilder.BaseType != null)
            {
                if (parameterTypes == null || parameterTypes.Count() == 0)
                {
                    method = Context.TypeBuilder.BaseType.GetMethods().FirstOrDefault(s => s.Name == methodName && s.GetParameters().Length == 0 && (s.ReturnType == ReturnType || (ReturnType == null && s.ReturnType == typeof(void))));
                }
                else if (parameterTypes.Count(s => s == null) == 0)
                {
                    method = Context.TypeBuilder.BaseType.GetMethods().FirstOrDefault(s => s.Name == methodName && IsEquals(s.GetParameters().Select(t => t.ParameterType).ToArray(), parameterTypes.ToArray()) && (s.ReturnType == ReturnType || (ReturnType == null && s.ReturnType == typeof(void))));
                }

                if (method != null && !method.IsVirtual)
                {
                    throw new InvalidOperationException(SR.GetString(SRKind.MethodNotOverride));
                }
            }

            //在实现的接口中查找方法
            var interfaceTypes = Context.TypeBuilder.InterfaceTypes
                .Union(Context.TypeBuilder.InterfaceTypes.SelectMany(s => s.GetInterfaces()))
                .Distinct().ToList();

            if (method == null && interfaceTypes.Count != 0)
            {
                foreach (var type in interfaceTypes)
                {
                    method = type.GetMethod(methodName, parameterTypes == null ? Type.EmptyTypes : parameterTypes.ToArray());
                    if (method != null)
                    {
                        break;
                    }
                }
            }

            return method;
        }

        private MethodAttributes GetMethodAttributes(VisualDecoration visual = VisualDecoration.Public, CallingDecoration calling = CallingDecoration.Standard)
        {
            var attributes = Context.TypeBuilder.GetMethodAttributes();

            switch (calling)
            {
                case CallingDecoration.Abstract:
                    attributes |= MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.NewSlot;
                    break;
                case CallingDecoration.Virtual:
                    attributes |= MethodAttributes.Virtual | MethodAttributes.NewSlot;
                    break;
                case CallingDecoration.Sealed:
                    attributes |= MethodAttributes.Final;
                    break;
                case CallingDecoration.Static:
                    attributes |= MethodAttributes.Static;
                    break;
                case CallingDecoration.ExplicitImpl:
                    attributes |= MethodAttributes.Private | MethodAttributes.Final;
                    break;
            }

            switch (visual)
            {
                case VisualDecoration.Internal:
                    attributes |= MethodAttributes.Assembly;
                    break;
                case VisualDecoration.Public:
                    if (calling != CallingDecoration.ExplicitImpl)
                    {
                        attributes |= MethodAttributes.Public;
                    }
                    break;
                case VisualDecoration.Protected:
                    attributes |= MethodAttributes.Family;
                    break;
            }

            return attributes;
        }

        private MethodAttributes GetMethodAttributes(string methodName, IEnumerable<Type> parameterTypes, VisualDecoration visual, CallingDecoration calling)
        {
            var method = FindMethod(methodName, parameterTypes);
            var isOverride = method != null && method.IsVirtual;
            var isInterface1 = isOverride && method.DeclaringType.IsInterface;
            var isBaseType = isOverride && method.DeclaringType == Context.TypeBuilder.BaseType;
            if (method != null)
            {
                CheckGenericMethod(method);
                Context.BaseMethod = method;
            }

            var attrs = GetMethodAttributes(visual, calling);
            if (isOverride)
            {
                attrs |= MethodAttributes.Virtual;

                //去掉 NewSlot
                if (isBaseType && attributes.HasFlag(MethodAttributes.NewSlot))
                {
                    attrs &= ~MethodAttributes.NewSlot;
                }
                else if (isInterface1)
                {
                    //如果没有传入 calling，则加 Final 去除上面定义的 Virtual
                    if (calling == CallingDecoration.Standard)
                    {
                        attrs |= MethodAttributes.Final;
                    }

                    attrs |= MethodAttributes.NewSlot;
                }
            }
            else if (method != null)
            {
            }

            return attrs;
        }

        private void CheckGenericMethod(MethodInfo method)
        {
            if (method.IsGenericMethod)
            {
                var types = method.GetGenericArguments();
                var list = types.Select(type => type.Name).ToList();
                if (method.ReturnType.IsGenericType)
                {
                    var index = list.IndexOf(method.ReturnType.Name);
                    if (index >= 0)
                    {
                        list.Add(list[index]);
                    }
                }

                GenericArguments = list.ToArray();
                if (methodBuilder != null)
                {
                    ProcessGenericMethod();
                }
            }
        }

        private void ProcessGenericMethod()
        {
            if (GenericArguments == null || methodBuilder == null)
            {
                return;
            }

            var array = new Dictionary<int, int>();
            var list = new List<string>();
            var l = 0;
            var v = Math.Min(GenericArguments.Length, ParameterTypes.Length);
            for (var i = 0; i < v; i++)
            {
                if (!string.IsNullOrEmpty(GenericArguments[i]))
                {
                    array.Add(i, l++);
                    list.Add(GenericArguments[i]);
                }
            }

            var gpas = methodBuilder.DefineGenericParameters(list.ToArray());
            foreach (var kvp in array)
            {
                ParameterTypes[kvp.Key] = gpas[kvp.Value];
            }

            MethodBuilder.SetParameters(ParameterTypes);
            if (GenericArguments.Length == ParameterTypes.Length + 1)
            {
                var index = list.IndexOf(GenericArguments[GenericArguments.Length - 1]);
                ReturnType = gpas[index];
            }
        }

        private void InitBuilder()
        {
            var isOverride = Calling == CallingDecoration.ExplicitImpl && Context.BaseMethod != null;

            if (isOverride)
            {
                methodBuilder = Context.TypeBuilder.TypeBuilder.DefineMethod(string.Concat(Context.BaseMethod.DeclaringType.Name, ".", Name), attributes);
            }
            else
            {
                methodBuilder = Context.TypeBuilder.TypeBuilder.DefineMethod(Name, attributes);
            }

            Context.Emitter = new EmitHelper(methodBuilder.GetILGenerator(), methodBuilder);
            if (ParameterTypes != null)
            {
                ProcessGenericMethod();
                methodBuilder.SetParameters(ParameterTypes);
            }

            if (ReturnType != null)
            {
                methodBuilder.SetReturnType(ReturnType);
            }

            if (action != null)
            {
                action(Context);
            }
            else
            {
                Context.Emitter.ret();
            }

            if (isOverride)
            {
                Context.TypeBuilder.TypeBuilder.DefineMethodOverride(methodBuilder, Context.BaseMethod);
            }
        }

        private static bool IsEquals(Type[] types1, Type[] types2)
        {
            if (types1.Length != types2.Length)
            {
                return false;
            }

            for (var i = 0; i < types1.Length; i++)
            {
                if (types1[i] != types2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
