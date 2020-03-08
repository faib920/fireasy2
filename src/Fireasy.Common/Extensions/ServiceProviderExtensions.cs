// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
using System;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// <see cref="IServiceProvider"/> 接口的扩展。
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// 尝试从 <see cref="IServiceProvider"/> 实例中获取指定的服务，如果没有则使用 <paramref name="creator"/> 函数创建一个服务实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvier"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public static T TryGetService<T>(this IServiceProvider serviceProvier, Func<T> creator = null)
        {
            if (serviceProvier != null)
            {
                var service = serviceProvier.GetService(typeof(T));
                if (service != null)
                {
                    return (T)service;
                }
            }

            return creator == null ? default : creator();
        }

        /// <summary>
        /// 尝试从 <see cref="IServiceProvider"/> 实例中获取指定的服务，如果没有则使用 <paramref name="creator"/> 函数创建一个服务实例。
        /// </summary>
        /// <param name="serviceProvier"></param>
        /// <param name="serviceType"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public static object TryGetService(this IServiceProvider serviceProvier, Type serviceType, Func<object> creator = null)
        {
            if (serviceProvier != null)
            {
                return serviceProvier.GetService(serviceType);
            }

            return creator == null ? default : creator();
        }

    }
}
