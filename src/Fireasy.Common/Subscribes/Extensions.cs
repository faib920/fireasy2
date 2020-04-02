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
using System.Linq;
using System.Reflection;

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
        public static ISubscribeManager Discovery(this ISubscribeManager subscribeMgr, Assembly assembly)
        {
            if (subscribeMgr != null && assembly != null)
            {
                var container = ContainerUnity.GetContainer();

                Helper.Discovery(subscribeMgr, assembly, type => container.Register(type, type, Lifetime.Transient), type => container.Resolve(type));
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
        internal static void Discovery(ISubscribeManager subscribeMgr, Assembly assembly, Action<Type> register, Func<Type, object> activator)
        {
            foreach (var type in assembly.GetExportedTypes().Where(s => !s.IsInterface && !s.IsAbstract))
            {
                Type subType = null;
                if ((subType = type.GetInterfaces().FirstOrDefault(s => s.IsGenericType && s.GetGenericTypeDefinition() == typeof(ISubscriber<>))) != null)
                {
                    register(type);

                    subType = subType.GetGenericArguments()[0];
                    subscribeMgr.AddSubscriber(subType, new Action<object>(o =>
                        {
                            var method = type.GetMethod("Accept");
                            if (method != null)
                            {
                                var instance = activator(type);
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
                    register(type);

                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
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
                                var instance = activator(type);
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

#if NETSTANDARD
namespace Microsoft.Extensions.DependencyInjection
{
    using Fireasy.Common;
    using Fireasy.Common.Subscribes;

    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 在指定的程序集中发现 <see cref="ISubscriber{TSubject}"/> 及 <see cref="ISubscribeHandler"/> 的实现。
        /// </summary>
        /// <param name="subscribeMgr"></param>
        /// <param name="services"></param>
        /// <param name="assembly">指定的程序集。</param>
        /// <returns></returns>
        public static ISubscribeManager Discovery(this ISubscribeManager subscribeMgr, IServiceCollection services, Assembly assembly)
        {
            if (subscribeMgr != null && services != null && assembly != null)
            {
                Helper.Discovery(subscribeMgr, assembly, type => services.AddScoped(type, type), type => services.AddUnity().GetRequiredService(type));
            }

            return subscribeMgr;
        }
    }
}
#endif