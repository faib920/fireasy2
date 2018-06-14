// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD2_0
using Fireasy.Common.Caching;
using Fireasy.Common.Caching.Configuration;
using Fireasy.Common.Configuration;
using Fireasy.Common.Ioc;
using Fireasy.Common.Ioc.Configuration;
using Fireasy.Common.Ioc.Registrations;
using Fireasy.Common.Logging;
using Fireasy.Common.Logging.Configuration;
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
                    services.AddSingleton(singReg.ServiceType, singReg.ComponentType);
                }
                else if (reg.GetType().IsGenericType && reg.GetType().GetGenericTypeDefinition() == typeof(FuncRegistration<>))
                {
                    services.AddTransient(reg.ServiceType, s => reg.Resolve());
                }
                else
                {
                    services.AddTransient(reg.ServiceType, reg.ComponentType);
                }
            }

            return services;
        }
    }

    internal class ConfigurationBinder
    {
        internal static void Bind(IServiceCollection services, IConfiguration configuration)
        {
            ConfigurationUnity.Bind<LoggingConfigurationSection>(configuration);
            ConfigurationUnity.Bind<CachingConfigurationSection>(configuration);
            ConfigurationUnity.Bind<ContainerConfigurationSection>(configuration);

            if (services != null)
            {
                services.AddSingleton(typeof(ILogger), s => LoggerFactory.CreateLogger());
                services.AddSingleton(typeof(ICacheManager), s => CacheManagerFactory.CreateManager());
            }
        }
    }
}
#endif