// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
using Fireasy.Common.Ioc;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#endif
using System;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// <see cref="IServiceProvider"/> 接口的扩展。
    /// </summary>
    public static class ServiceProviderExtension
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
                try
                {
                    var service = serviceProvier.GetService(typeof(T));
                    if (service != null)
                    {
                        return (T)service;
                    }
                }
                catch (ObjectDisposedException)
                {
                    Tracer.Error($"{serviceProvier} is disposed");
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
                try
                {
                    return serviceProvier.GetService(serviceType);
                }
                catch (ObjectDisposedException)
                {
                    Tracer.Error($"{serviceProvier} is disposed");
                }
            }

            return creator == null ? default : creator();
        }

        /// <summary>
        /// 如果对象实现了 <see cref="IServiceProviderAccessor"/>，则尝试获取 <see cref="IServiceProvider"/> 实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IServiceProvider TryGetServiceProvider<T>(this T obj)
        {
            if (obj is IServiceProviderAccessor accessor)
            {
                return accessor.ServiceProvider;
            }

            return null;
        }

        /// <summary>
        /// 如果对象实现了 <see cref="IServiceProviderAccessor"/>，则尝试对实例附加 <see cref="IServiceProvider"/> 实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static T TrySetServiceProvider<T>(this T obj, IServiceProvider serviceProvider)
        {
            if (obj == null || serviceProvider == null)
            {
                return obj;
            }

            if (obj is IServiceProviderAccessor accessor && accessor.ServiceProvider != serviceProvider)
            {
                accessor.ServiceProvider = serviceProvider;
            }

            return obj;
        }

        /// <summary>
        /// 如果对象实现了 <see cref="IServiceProviderAccessor"/>，则尝试使用 <see cref="Container"/> 作为实例的 <see cref="IServiceProvider"/>。
        /// </summary>
        /// <returns></returns>
        public static T TryUseContainer<T>(this T obj)
        {
            if (obj == null)
            {
                return obj;
            }

            if (obj is IServiceProviderAccessor accessor && accessor.ServiceProvider == null)
            {
                accessor.ServiceProvider = ContainerUnity.GetContainer();
            }

            return obj;
        }

        /// <summary>
        /// 在当前请求内创建一个范围的 <see cref="IServiceProvider"/> 实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
#if NETSTANDARD
        public static IServiceScope TryCreateScope(this IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IServiceScopeFactory)) is IServiceScopeFactory factory)
            {
                return factory.CreateScope();
            }
            else if (serviceProvider is IResolver resolver)
            {
                return resolver.CreateScope();
            }
#else
        public static IResolver TryCreateScope(this IServiceProvider serviceProvider)
        {
            if (serviceProvider is IResolver resolver)
            {
                return resolver.CreateScope();
            }

#endif

            return null;
        }

#if NETSTANDARD
        /// <summary>
        /// 记录跟踪性日志。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="message"></param>
        public static void LogInformation(this IServiceProvider serviceProvider, string message)
        {
            var logger = serviceProvider.TryGetService<ILogger<Tracer>>();
            if (logger != null)
            {
                logger.LogInformation(message);
            }
        }

        /// <summary>
        /// 记录错误性日志。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="message"></param>
        public static void LogError(this IServiceProvider serviceProvider, string message)
        {
            var logger = serviceProvider.TryGetService<ILogger<Tracer>>();
            if (logger != null)
            {
                logger.LogError(message);
            }
        }
#endif
    }
}
