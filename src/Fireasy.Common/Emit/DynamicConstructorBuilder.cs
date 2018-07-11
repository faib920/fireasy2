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
    /// 用于创建一个动态的构造器。
    /// </summary>
    public class DynamicConstructorBuilder : DynamicBuilder
    {
        private ConstructorBuilder constrBuilder;
        private readonly Action<BuildContext> action;
        private readonly MethodAttributes attributes;
        private readonly List<string> parameters = new List<string>();

        internal DynamicConstructorBuilder(BuildContext context, Type[] parameterTypes, VisualDecoration visual = VisualDecoration.Public, CallingDecoration calling = CallingDecoration.Standard, Action<BuildContext> ilCoding = null)
            : base(visual, calling)
        {
            Context = new BuildContext(context) { ConstructorBuilder = this };
            ParameterTypes = parameterTypes;
            if (ilCoding == null)
            {
                if (context.TypeBuilder.BaseType != null)
                {
                    var baseCon = parameterTypes == null ?
                        context.TypeBuilder.BaseType.GetConstructors().FirstOrDefault() :
                        context.TypeBuilder.BaseType.GetConstructor(parameterTypes);

                    if (baseCon != null)
                    {
                        ilCoding = c => c.Emitter.ldarg_0
                            .Assert(parameterTypes != null, b => b.For(0, parameterTypes.Length, (e, i) => e.ldarg(i + 1)))
                            .call(baseCon).ret();
                    }
                }
                else
                {
                    ilCoding = c => c.Emitter.ret();
                }
            }

            this.action = ilCoding;
            attributes = GetMethodAttributes(visual, calling);
            InitBuilder();
        }

        /// <summary>
        /// 获取参数类型。
        /// </summary>
        public Type[] ParameterTypes { get; private set; }

        /// <summary>
        /// 追加新的 MSIL 代码到构造器中。
        /// </summary>
        /// <param name="ilCoding"></param>
        /// <returns></returns>
        public DynamicConstructorBuilder AppendCode(Action<EmitHelper> ilCoding)
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
        public DynamicConstructorBuilder OverwriteCode(Action<EmitHelper> ilCoding)
        {
            var builderField = typeof(ConstructorBuilder).GetField("m_methodBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
            var builder = builderField.GetValue(constrBuilder);
            var field = typeof(MethodBuilder).GetField("m_ilGenerator", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var cons = typeof(ILGenerator).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
                field.SetValue(builder, cons.Invoke(new[] { builder }));
                Context.Emitter = new EmitHelper(constrBuilder.GetILGenerator());
            }

            return AppendCode(ilCoding);
        }

        /// <summary>
        /// 定义一个参数。
        /// </summary>
        /// <param name="name">参数的名称。</param>
        /// <returns></returns>
        public DynamicConstructorBuilder DefineParameter(string name)
        {
            ConstructorBuilder.DefineParameter(parameters.Count + 1, ParameterAttributes.None, name);
            parameters.Add(name);
            return this;
        }

        /// <summary>
        /// 定义一个参数。
        /// </summary>
        /// <param name="name">参数的名称。</param>
        /// <returns></returns>
        public DynamicConstructorBuilder DefineParameter(string name, bool hasDefaultValue = false, object defaultValue = null)
        {
            var parameterBuilder = ConstructorBuilder.DefineParameter(parameters.Count + 1, hasDefaultValue ? ParameterAttributes.HasDefault : ParameterAttributes.None, name);
            if (hasDefaultValue)
            {
                parameterBuilder.SetConstant(defaultValue);
            }

            parameters.Add(name);
            return this;
        }

        /// <summary>
        /// 获取 <see cref="ConstructorBuilder"/> 对象。
        /// </summary>
        /// <returns></returns>
        public ConstructorBuilder ConstructorBuilder
        {
            get { return constrBuilder; }
        }

        /// <summary>
        /// 设置一个 <see cref="CustomAttributeBuilder"/> 对象到当前实例关联的 <see cref="ConstructorBuilder"/> 对象。
        /// </summary>
        /// <param name="customBuilder">一个 <see cref="CustomAttributeBuilder"/> 对象。</param>
        protected override void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            ConstructorBuilder.SetCustomAttribute(customBuilder);
        }

        private void InitBuilder()
        {
            var isDefault = (ParameterTypes == null || ParameterTypes.Length == 0) && action == null;
            constrBuilder = isDefault ?
                Context.TypeBuilder.TypeBuilder.DefineDefaultConstructor(attributes) :
                Context.TypeBuilder.TypeBuilder.DefineConstructor(attributes, CallingConventions.Standard, ParameterTypes);
            if (!isDefault)
            {
                Context.Emitter = new EmitHelper(constrBuilder.GetILGenerator());
            }

            if (action != null)
            {
                action(Context);
            }
        }

        private MethodAttributes GetMethodAttributes(VisualDecoration visual = VisualDecoration.Public, CallingDecoration calling = CallingDecoration.Standard)
        {
            var attrs = Context.TypeBuilder.GetMethodAttributes();

            switch (calling)
            {
                case CallingDecoration.Abstract:
                    attrs |= MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.NewSlot;
                    break;
                case CallingDecoration.Virtual:
                    attrs |= MethodAttributes.Virtual | MethodAttributes.NewSlot;
                    break;
                case CallingDecoration.Sealed:
                    attrs |= MethodAttributes.Final;
                    break;
                case CallingDecoration.Static:
                    attrs |= MethodAttributes.Static;
                    break;
            }

            switch (visual)
            {
                case VisualDecoration.Internal:
                    attrs |= MethodAttributes.Assembly;
                    break;
                case VisualDecoration.Public:
                    attrs |= MethodAttributes.Public;
                    break;
                case VisualDecoration.Protected:
                    attrs |= MethodAttributes.Family;
                    break;
            }

            return attrs;
        }
    }
}
