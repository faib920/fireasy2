// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD2_0
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 为 <see cref="IServiceCollection"/> 提供一个 <see cref="IServiceProvider"/> 实例。
    /// </summary>
    public static class ServiceUnity
    {
        private static object locker = new object();

        /// <summary>
        /// 获取当前的 <see cref="IServiceProvider"/> 实例。
        /// </summary>
        public static IServiceProvider Provider { get; private set; }

        /// <summary>
        /// 将 <paramref name="services"/> 注册到当前的程序集中，以便通过使用 Provider 来获取服务。
        /// </summary>
        /// <param name="services"></param>
        public static void AddUnity(this IServiceCollection services)
        {
            lock (locker)
            {
                if (Provider == null)
                {
                    Provider = services.BuildServiceProvider();
                }
            }
        }
    }
}
#endif
