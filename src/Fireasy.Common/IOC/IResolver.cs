// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Fireasy.Common.Ioc
{
    /// <summary>
    /// 对象的反转提供者。
    /// </summary>
    public interface IResolver : IDisposable
#if NETSTANDARD
        , IServiceScope
#endif
    {
        /// <summary>
        /// 获取 <see cref="IServiceProvider"/> 对象。
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// 创建一个范围，属于 <see cref="Lifetime.Scoped"/> 的对象在此范围内共享。
        /// </summary>
        /// <returns></returns>
        IResolver CreateScope();

        /// <summary>
        /// 从容器中反转出一个对象。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// 从容器中反转出一个对象。
        /// </summary>
        /// <returns></returns>
        TService Resolve<TService>() where TService : class;

        /// <summary>
        /// 获取 <paramref name="serviceType"/> 在容器中注册的关系。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        IEnumerable<IRegistration> GetRegistrations(Type serviceType);
    }
}
