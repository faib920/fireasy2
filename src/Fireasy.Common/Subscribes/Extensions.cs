// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Fireasy.Common.Subscribes
{
    public static class Extensions
    {
        /// <summary>
        /// 在指定的程序集中发现 <see cref="ISubscriber{TSubject}"/> 及 <see cref="ISubscribeHandler"/> 的实现。
        /// </summary>
        /// <param name="subscribeMgr"><see cref="ISubscribeManager"/> 实例。</param>
        /// <param name="assembly">指定的程序集。</param>
        /// <returns></returns>
        public static ISubscribeManager AddSubscribers(this ISubscribeManager subscribeMgr, Assembly assembly)
        {
            if (subscribeMgr != null && assembly != null)
            {
                var container = ContainerUnity.GetContainer();
                var types = Helper.DiscoveryTypes(assembly);
                types.ForEach(s => container.Register(s, s, Lifetime.Scoped));

                Helper.RegisterSubscribers(container, subscribeMgr, types, (sp, type) => sp.GetService(type));
            }

            return subscribeMgr;
        }

        /// <summary>
        /// 添加一个订阅者实例。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscribeMgr"><see cref="ISubscribeManager"/> 实例。</param>
        /// <param name="subscriber">主题订阅者。</param>
        /// <returns></returns>
        public static ISubscribeManager AddSubscriber<TSubject>(this ISubscribeManager subscribeMgr, ISubscriber<TSubject> subscriber) where TSubject : class
        {
            Guard.ArgumentNull(subscribeMgr, nameof(subscribeMgr));
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            subscribeMgr.AddSubscriber<TSubject>(subject => subscriber.Accept(subject));
            return subscribeMgr;
        }
    }

    internal class Helper
    {
        /// <summary>
        /// 在指定的程序集中发现所有订阅器处理类型。
        /// </summary>
        /// <param name="assembly">程序集。</param>
        /// <returns></returns>
        internal static IEnumerable<Type> DiscoveryTypes(Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes().Where(s => !s.IsInterface && !s.IsAbstract))
            {
                Type subType = null;
                if ((subType = type.GetInterfaces().FirstOrDefault(s => s.IsGenericType && s.GetGenericTypeDefinition() == typeof(ISubscriber<>))) != null)
                {
                    yield return type;
                }
                else if (type.IsImplementInterface(typeof(ISubscribeHandler)))
                {
                    yield return type;
                }
            }
        }

#if NETSTANDARD
        /// <summary>
        /// 发现所有订阅器处理类型。
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        internal static IEnumerable<Type> DiscoveryTypes(IServiceCollection services)
        {
            foreach (var desc in services)
            {
                Type subType = null;
                if ((subType = desc.ServiceType.GetInterfaces().FirstOrDefault(s => s.IsGenericType && s.GetGenericTypeDefinition() == typeof(ISubscriber<>))) != null)
                {
                    yield return desc.ServiceType;
                }
                else if (desc.ServiceType.IsImplementInterface(typeof(ISubscribeHandler)))
                {
                    yield return desc.ServiceType;
                }
            }
        }
#endif

        /// <summary>
        /// 注册订阅器。
        /// </summary>
        /// <param name="serviceProvider">应用程序服务实例。</param>
        /// <param name="subscribeMgr">指定订阅管理器。</param>
        /// <param name="types">可被注册为订阅器的类型集合。</param>
        /// <param name="activator">创建订阅器实例的方法，一般是从 <paramref name="serviceProvider"/> 里获取。</param>
        internal static void RegisterSubscribers(IServiceProvider serviceProvider, ISubscribeManager subscribeMgr, IEnumerable<Type> types, Func<IServiceProvider, Type, object> activator)
        {
            foreach (var type in types)
            {
                Type subType = null;
                if ((subType = type.GetInterfaces().FirstOrDefault(s => s.IsGenericType && s.GetGenericTypeDefinition() == typeof(ISubscriber<>))) != null)
                {
                    subType = subType.GetGenericArguments()[0];
                    subscribeMgr.AddSubscriber(subType, new Action<object>(o =>
                        {
                            var method = type.GetMethod(nameof(ISubscriber<string>.Accept));
                            if (method != null)
                            {
                                using var scope = serviceProvider.TryCreateScope();
                                var instance = activator(scope?.ServiceProvider ?? serviceProvider, type);
                                if (instance != null)
                                {
                                    method.FastInvoke(instance, new[] { o });
                                }

                                instance.TryDispose();
                            }
                        }));
                }
                else if (type.IsImplementInterface(typeof(ISubscribeHandler)))
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (method.DeclaringType != type)
                        {
                            continue;
                        }

                        var parameters = method.GetParameters();
                        if (parameters.Length != 1)
                        {
                            continue;
                        }

                        subscribeMgr.AddSubscriber(parameters[0].ParameterType, new Action<object>(o =>
                            {
                                using var scope = serviceProvider.TryCreateScope();
                                var instance = activator(scope?.ServiceProvider ?? serviceProvider, type);
                                if (instance != null)
                                {
                                    method.FastInvoke(instance, new[] { o });

                                    instance.TryDispose();
                                }
                            }));
                    }
                }
            }
        }
    }
}