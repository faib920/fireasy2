// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Emit
{
    /// <summary>
    /// 表示可以创建类。
    /// </summary>
    internal interface ITypeCreator
    {
        /// <summary>
        /// 用此生成器创建一个类。
        /// </summary>
        /// <returns></returns>
        Type CreateType();

        /// <summary>
        /// 获取或设置代理创建者。
        /// </summary>
        Func<Type> Creator { get; set; }
    }
}
