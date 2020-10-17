// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Ioc
{
    /// <summary>
    /// 实例初始化器。
    /// </summary>
    internal sealed class InstanceInitializer
    {
        /// <summary>
        /// 获取或设置服务类型。
        /// </summary>
        internal Type ServiceType { get; set; }

        /// <summary>
        /// 获取或设置该服务类型的实例。
        /// </summary>
        internal object Action { get; set; }
    }
}
