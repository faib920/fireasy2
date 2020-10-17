// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common
{
    /// <summary>
    /// 提供对 <see cref="IServiceProvider"/> 实例的访问。
    /// </summary>
    public interface IServiceProviderAccessor
    {
        /// <summary>
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        IServiceProvider ServiceProvider { get; set; }
    }
}
