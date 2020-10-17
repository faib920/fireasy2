// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using System;
using System.Reflection;

namespace Fireasy.Common
{
    public class CoreOptions
    {
        /// <summary>
        /// 获取或设置用于过滤程序集的函数。
        /// </summary>
        public Func<Assembly, bool> AssemblyFilter { get; set; }
    }
}
#endif
