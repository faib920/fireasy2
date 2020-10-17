// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fireasy.Common
{
    /// <summary>
    /// 为 <see cref="IServiceCollection"/> 提供一个 <see cref="IServiceProvider"/> 实例。
    /// </summary>
    public static class ServiceUnity
    {
        private static readonly object _locker = new object();

        /// <summary>
        /// 获取当前的 <see cref="IServiceProvider"/> 实例。
        /// </summary>
        public static IServiceProvider Provider { get; private set; }

        /// <summary>
        /// 将 <paramref name="services"/> 注册到当前的程序集中，以便通过使用 Provider 来获取服务。
        /// </summary>
        /// <param name="services"></param>
        public static IServiceProvider AddUnity(this IServiceCollection services)
        {
            lock (_locker)
            {
                return Provider = services.BuildServiceProvider();
            }
        }
    }
}
#endif
