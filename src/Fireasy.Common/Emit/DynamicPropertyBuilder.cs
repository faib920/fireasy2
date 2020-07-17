// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Fireasy.Common.Emit
{
    /// <summary>
    /// 用于创建一个动态的属性。
    /// </summary>
    public class DynamicPropertyBuilder : DynamicBuilder
    {
        private PropertyBuilder _propertyBuilder;
        private DynamicFieldBuilder _fieldBuilder;

        internal DynamicPropertyBuilder(BuildContext context, string propertyName, Type propertyType, VisualDecoration visual = VisualDecoration.Public, CallingDecoration calling = CallingDecoration.Standard)
            : base(visual, calling)
        {
            Context = new BuildContext(context) { PropertyBuilder = this };
            Name = propertyName;
            PropertyType = propertyType;
            InitBuilder();
        }

        /// <summary>
        /// 获取属性的名称。
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 获取属性的类型。
        /// </summary>
        public Type PropertyType { get; private set; }

        /// <summary>
        /// 获取当前的 <see cref="DynamicFieldBuilder"/>。
        /// </summary>
        /// <returns></returns>
        public DynamicFieldBuilder FieldBuilder
        {
            get
            {
                return _fieldBuilder ?? (_fieldBuilder = Context.TypeBuilder.DefineField(string.Format("m_<{0}>", Name), PropertyType));
            }
        }

        /// <summary>
        /// 获取当前的 <see cref="PropertyBuilder"/>。
        /// </summary>
        /// <returns></returns>
        public PropertyBuilder PropertyBuilder
        {
            get { return _propertyBuilder; }
        }

        /// <summary>
        /// 使用默认的过程定义属性的 Get 和 Set 方法体。
        /// </summary>
        /// <param name="field">指定一个属性相关的 <see cref="DynamicFieldBuilder"/>。</param>
        /// <returns>当前的 <see cref="DynamicTypeBuilder"/>。</returns>
        public DynamicPropertyBuilder DefineGetSetMethods(DynamicFieldBuilder field = null)
        {
            DefineGetMethodByField(Visual, Calling, fieldBuilder: field);
            DefineSetMethodByField(Visual, Calling, fieldBuilder: field);
            return this;
        }

        /// <summary>
        /// 定义属性的 Get 访问方法。
        /// </summary>
        /// <param name="visual">指定方法的可见性。</param>
        /// <param name="calling">指定方法的调用属性。</param>
        /// <param name="ilCoding">方法体的 IL 过程。</param>
        /// <returns>新的 <see cref="DynamicMethodBuilder"/>。</returns>
        public DynamicMethodBuilder DefineGetMethod(VisualDecoration visual = VisualDecoration.Public, CallingDecoration calling = CallingDecoration.Standard, Action<BuildContext> ilCoding = null)
        {
            var isInterface = Context.TypeBuilder is DynamicInterfaceBuilder;
            var method = new DynamicMethodBuilder(Context, string.Concat("get_", GetMethodName()), PropertyType, Type.EmptyTypes, visual, calling, ctx =>
                {
                    if (isInterface)
                    {
                        return;
                    }

                    if (ilCoding != null)
                    {
                        ilCoding(ctx);
                    }
                    else
                    {
                        ctx.Emitter.ldarg_0.ldfld(FieldBuilder.FieldBuilder).ret();
                    }
                });
            PropertyBuilder.SetGetMethod(method.MethodBuilder);
            return method;
        }

        /// <summary>
        /// 定义属性的 Get 访问方法。
        /// </summary>
        /// <param name="visual">指定方法的可见性。</param>
        /// <param name="calling">指定方法的调用属性。</param>
        /// <param name="fieldBuilder">指定一个属性相关的 <see cref="DynamicFieldBuilder"/>。</param>
        /// <returns>新的 <see cref="DynamicMethodBuilder"/>。</returns>
        public DynamicMethodBuilder DefineGetMethodByField(VisualDecoration visual = VisualDecoration.Public, CallingDecoration calling = CallingDecoration.Standard, DynamicFieldBuilder fieldBuilder = null)
        {
            var isInterface = Context.TypeBuilder is DynamicInterfaceBuilder;
            var method = new DynamicMethodBuilder(Context, string.Concat("get_", GetMethodName()), PropertyType, Type.EmptyTypes, visual, calling, ctx =>
                {
                    if (isInterface)
                    {
                        return;
                    }

                    if (fieldBuilder == null)
                    {
                        fieldBuilder = FieldBuilder;
                    }

                    ctx.Emitter.ldarg_0.ldfld(fieldBuilder.FieldBuilder).ret();
                });

            PropertyBuilder.SetGetMethod(method.MethodBuilder);
            return method;
        }

        /// <summary>
        /// 定义属性的 Set 访问方法。
        /// </summary>
        /// <param name="visual">指定方法的可见性。</param>
        /// <param name="calling">指定方法的调用属性。</param>
        /// <param name="ilCoding">方法体的 IL 过程。</param>
        /// <returns>新的 <see cref="DynamicMethodBuilder"/>。</returns>
        public DynamicMethodBuilder DefineSetMethod(VisualDecoration visual = VisualDecoration.Public, CallingDecoration calling = CallingDecoration.Standard, Action<BuildContext> ilCoding = null)
        {
            var isInterface = Context.TypeBuilder is DynamicInterfaceBuilder;
            var method = new DynamicMethodBuilder(Context, string.Concat("set_", GetMethodName()), null, new[] { PropertyType }, visual, calling, ctx =>
                {
                    if (isInterface)
                    {
                        return;
                    }

                    if (ilCoding != null)
                    {
                        ilCoding(ctx);
                    }
                    else
                    {
                        ctx.Emitter.ldarg_0.ldarg_1.stfld(FieldBuilder.FieldBuilder).ret();
                    }
                });
            PropertyBuilder.SetSetMethod(method.MethodBuilder);
            return method;
        }

        /// <summary>
        /// 定义属性的 Set 访问方法。
        /// </summary>
        /// <param name="visual">指定方法的可见性。</param>
        /// <param name="calling">指定方法的调用属性。</param>
        /// <param name="fieldBuilder">指定一个属性相关的 <see cref="DynamicFieldBuilder"/>。</param>
        /// <returns>新的 <see cref="DynamicMethodBuilder"/>。</returns>
        public DynamicMethodBuilder DefineSetMethodByField(VisualDecoration visual = VisualDecoration.Public, CallingDecoration calling = CallingDecoration.Standard, DynamicFieldBuilder fieldBuilder = null)
        {
            var isInterface = Context.TypeBuilder is DynamicInterfaceBuilder;
            var method = new DynamicMethodBuilder(Context, string.Concat("set_", GetMethodName()), null, new[] { PropertyType }, visual, calling, ctx =>
                {
                    if (isInterface)
                    {
                        return;
                    }

                    if (fieldBuilder == null)
                    {
                        fieldBuilder = FieldBuilder;
                    }

                    ctx.Emitter.ldarg_0.ldarg_1.stfld(fieldBuilder.FieldBuilder).ret();
                });
            PropertyBuilder.SetSetMethod(method.MethodBuilder);
            return method;
        }

        /// <summary>
        /// 设置一个 <see cref="CustomAttributeBuilder"/> 对象到当前实例关联的 <see cref="PropertyBuilder"/> 对象。
        /// </summary>
        /// <param name="customBuilder">一个 <see cref="CustomAttributeBuilder"/> 对象。</param>
        protected override void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            PropertyBuilder.SetCustomAttribute(customBuilder);
        }

        private PropertyAttributes GetPropertyAttributes()
        {
            return Context.TypeBuilder.GetPropertyAttributes();
        }

        private void InitBuilder()
        {
            _propertyBuilder = Context.TypeBuilder.TypeBuilder.DefineProperty(Name, GetPropertyAttributes(), PropertyType, null);
        }

        private string GetMethodName()
        {
            var dot = Name.IndexOf(".");
            return dot == -1 ? Name : Name.Substring(dot + 1);
        }
    }
}