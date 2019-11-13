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
using Fireasy.Common.Extensions;

namespace Fireasy.Common.Emit
{
    /// <summary>
    /// 用于创建一个动态的枚举。
    /// </summary>
    public class DynamicEnumBuilder : DynamicBuilder, ITypeCreator
    {
        private readonly TypeAttributes attributes;
        private EnumBuilder enumBuilder;
        private Type innerType;

        internal DynamicEnumBuilder(BuildContext context, string enumName, Type underlyingType, VisualDecoration visual = VisualDecoration.Public)
             : base(visual, CallingDecoration.Standard)
        {
            Context = new BuildContext(context) { EnumBuilder = this };
            EnumName = enumName;
            UnderlyingType = underlyingType;
            attributes = GetTypeAttributes(visual);
            InitBuilder();
        }

        /// <summary>
        /// 获取枚举的名称。
        /// </summary>
        public string EnumName { get; private set; }

        /// <summary>
        /// 获取枚举的类型。
        /// </summary>
        public Type UnderlyingType { get; private set; }

        /// <summary>
        /// 获取 <see cref="EnumBuilder"/> 对象。
        /// </summary>
        /// <returns></returns>
        public EnumBuilder EnumBuilder
        {
            get { return enumBuilder; }
        }

        /// <summary>
        /// 定义一个枚举值。
        /// </summary>
        /// <param name="literalName">标签的名称。</param>
        /// <param name="value">枚举值。</param>
        /// <returns></returns>
        public DynamicFieldBuilder DefineLiteral(string literalName, object value)
        {
            return new DynamicFieldBuilder(Context, literalName, UnderlyingType, value.ToType(UnderlyingType));
        }

        /// <summary>
        /// 创建动态的枚举类型。
        /// </summary>
        /// <returns></returns>
        public Type CreateType()
        {
            if (innerType != null)
            {
                return innerType;
            }
#if !NETSTANDARD
            innerType = enumBuilder.CreateType();
#else
            innerType = enumBuilder.CreateTypeInfo();
#endif
            return innerType;
        }

        /// <summary>
        /// 设置一个 <see cref="CustomAttributeBuilder"/> 对象到当前实例关联的 <see cref="EnumBuilder"/> 对象。
        /// </summary>
        /// <param name="customBuilder">一个 <see cref="CustomAttributeBuilder"/> 对象。</param>
        protected override void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            enumBuilder.SetCustomAttribute(customBuilder);
        }

        private TypeAttributes GetTypeAttributes(VisualDecoration visual)
        {
            var attrs = TypeAttributes.Class;
            switch (visual)
            {
                case VisualDecoration.Internal:
                    break;
                case VisualDecoration.Public:
                    attrs |= TypeAttributes.Public;
                    break;
            }

            return attrs;
        }

        private void InitBuilder()
        {
            enumBuilder = Context.AssemblyBuilder.ModuleBuilder.DefineEnum(EnumName, attributes, UnderlyingType);
        }
    }
}
