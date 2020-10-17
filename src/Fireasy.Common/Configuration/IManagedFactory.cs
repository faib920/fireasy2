// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Configuration
{
    /// <summary>
    /// 一个外部托管工厂接口。
    /// </summary>
    public interface IManagedFactory
    {
        /// <summary>
        /// 创建实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="name">配置名称。</param>
        /// <returns></returns>
        object CreateInstance(IServiceProvider serviceProvider, string name);
    }
}
