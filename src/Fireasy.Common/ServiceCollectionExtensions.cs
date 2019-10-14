// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Aop;
using Fireasy.Common.Caching;
using Fireasy.Common.Caching.Configuration;
using Fireasy.Common.Composition.Configuration;
using Fireasy.Common.Configuration;
using Fireasy.Common.Ioc;
using Fireasy.Common.Ioc.Configuration;
using Fireasy.Common.Ioc.Registrations;
using Fireasy.Common.Logging;
using Fireasy.Common.Logging.Configuration;
using Fireasy.Common.Subscribes;
using Fireasy.Common.Subscribes.Configuration;
using Fireasy.Common.Threading;
using Fireasy.Common.Threading.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 使 <see cref="IServiceCollection"/> 能够使用 Fireasy 框架中的配置。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddFireasy(this IServiceCollection services, IConfiguration configuration, Action<Fireasy.Common.CoreOptions> setupAction = null)
        {
            ConfigurationUnity.Bind(Assembly.GetCallingAssembly(), configuration, services);

            var options = new Fireasy.Common.CoreOptions();
            setupAction?.Invoke(options);

            return services;
        }

        /// <summary>
        /// 在 <see cref="IServiceCollection"/> 上注册 Fireasy 容器里的定义。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IServiceCollection AddIoc(this IServiceCollection services, Container container = null)
        {
            container = container ?? ContainerUnity.GetContainer();
            foreach (AbstractRegistration reg in container.GetRegistrations())
            {
                if (reg is SingletonRegistration singReg)
                {
                    services.AddSingleton(singReg.ServiceType, CheckAopProxyType(singReg.ImplementationType));
                }
                else if (reg.GetType().IsGenericType && reg.GetType().GetGenericTypeDefinition() == typeof(FuncRegistration<>))
                {
                    services.AddTransient(reg.ServiceType, s => reg.Resolve());
                }
                else
                {
                    services.AddTransient(reg.ServiceType, CheckAopProxyType(reg.ImplementationType));
                }
            }

            return services;
        }

        /// <summary>
        /// 检查是否支持 <see cref="IAopSupport"/> 接口。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type CheckAopProxyType(Type type)
        {
            if (typeof(IAopSupport).IsAssignableFrom(type) && !typeof(IAopImplement).IsAssignableFrom(type))
            {
                return AspectFactory.GetProxyType(type);
            }

            return type;
        }
    }

    internal class ConfigurationBinder
    {
        internal static void Bind(IServiceCollection services, IConfiguration configuration)
        {
            ConfigurationUnity.Bind<LoggingConfigurationSection>(configuration);
            ConfigurationUnity.Bind<CachingConfigurationSection>(configuration);
            ConfigurationUnity.Bind<ContainerConfigurationSection>(configuration);
            ConfigurationUnity.Bind<LockerConfigurationSection>(configuration);
            ConfigurationUnity.Bind<SubscribeConfigurationSection>(configuration);
            ConfigurationUnity.Bind<ImportConfigurationSection>(configuration);

            if (services != null)
            {
                services.AddLogger().AddCaching().AddSubscriber().AddLocker();
            }
        }
    }
}
#endif