// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Reflection;

namespace Fireasy.Common.Emit
{
    /// <summary>
    /// Emit 的上下文对象。
    /// </summary>
    public sealed class BuildContext
    {
        internal BuildContext()
        {
        }

        internal BuildContext(BuildContext context)
        {
            AssemblyBuilder = context.AssemblyBuilder;
            TypeBuilder = context.TypeBuilder;
            EnumBuilder = context.EnumBuilder;
            PropertyBuilder = context.PropertyBuilder;
            MethodBuilder = context.MethodBuilder;
            ConstructorBuilder = context.ConstructorBuilder;
            Emitter = context.Emitter;
        }

        /// <summary>
        /// 获取 <see cref="DynamicAssemblyBuilder"/>。
        /// </summary>
        public DynamicAssemblyBuilder AssemblyBuilder { get; internal set; }

        /// <summary>
        /// 获取 <see cref="DynamicTypeBuilder"/>。
        /// </summary>
        public DynamicTypeBuilder TypeBuilder { get; internal set; }

        /// <summary>
        /// 获取 <see cref="DynamicPropertyBuilder"/>。
        /// </summary>
        public DynamicPropertyBuilder PropertyBuilder { get; internal set; }

        /// <summary>
        /// 获取 <see cref="DynamicMethodBuilder"/>。
        /// </summary>
        public DynamicMethodBuilder MethodBuilder { get; internal set; }

        /// <summary>
        /// 获取 <see cref="DynamicEnumBuilder"/>。
        /// </summary>
        public DynamicEnumBuilder EnumBuilder { get; internal set; }

        /// <summary>
        /// 获取 <see cref="DynamicConstructorBuilder"/>。
        /// </summary>
        public DynamicConstructorBuilder ConstructorBuilder { get; internal set; }

        /// <summary>
        /// 获取重载父类的方法。
        /// </summary>
        public MethodInfo BaseMethod { get; internal set; }

        /// <summary>
        /// 获取 <see cref="EmitHelper"/>。
        /// </summary>
        public EmitHelper Emitter { get; internal set; }
    }
}
