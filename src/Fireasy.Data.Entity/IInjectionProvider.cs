// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Emit;
using Fireasy.Data.Provider;
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 用于在编译实体类的代理类时，向构造器注入部分代码。
    /// </summary>
    public interface IInjectionProvider : IProviderService
    {
        /// <summary>
        /// 注入代码。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="assemblyBuilder"></param>
        /// <param name="typeBuilder"></param>
        void Inject(Type entityType, DynamicAssemblyBuilder assemblyBuilder, DynamicTypeBuilder typeBuilder);
    }
}
