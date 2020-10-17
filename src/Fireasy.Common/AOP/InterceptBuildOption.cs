// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Emit;
using System;

namespace Fireasy.Common.Aop
{
    /// <summary>
    /// 注入构造的选项。
    /// </summary>
    public sealed class InterceptBuildOption
    {
        /// <summary>
        /// 获取或设置类名称的格式器。使用 {0} 格式化。
        /// </summary>
        public string TypeNameFormatter { get; set; }

        /// <summary>
        /// 获取或设置 <see cref="DynamicAssemblyBuilder"/> 的初始化器。
        /// </summary>
        public Action<DynamicAssemblyBuilder> AssemblyInitializer { get; set; }

        /// <summary>
        /// 获取或设置 <see cref="DynamicTypeBuilder"/> 的初始化器。
        /// </summary>
        public Action<DynamicTypeBuilder> TypeInitializer { get; set; }

        /// <summary>
        /// 获取或设置指定的 <see cref="DynamicAssemblyBuilder"/> 对象。
        /// </summary>
        public DynamicAssemblyBuilder AssemblyBuilder { get; set; }
    }
}
