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
    /// 提供对服务类的登记以及解析。
    /// </summary>
    public interface IRegistration
    {
        /// <summary>
        /// 获取登记的服务类型，一般为一个接口或抽象类。
        /// </summary>
        Type ServiceType { get; }

        /// <summary>
        /// 从容器里解析出实现类的实例。
        /// </summary>
        /// <returns></returns>
        object Resolve();
    }
}
