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
using System.Collections.Generic;
using System.Linq;
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
            var assemblies = new List<Assembly>();

            FindReferenceAssemblies(Assembly.GetCallingAssembly(), assemblies);

            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType("Microsoft.Extensions.DependencyInjection.ConfigurationBinder");
                if (type != null)
                {
                    var method = type.GetMethod("Bind", BindingFlags.Static | BindingFlags.NonPublic);
                    if (method != null)
                    {
                        method.Invoke(null, new object[] { services, configuration });
                    }
                }
            }

            var options = new Fireasy.Common.CoreOptions();
            setupAction?.Invoke(options);

            assemblies.Clear();

            return services;
        }

        private static void FindReferenceAssemblies(Assembly assembly, List<Assembly> assemblies)
        {
            foreach (var asb in assembly.GetReferencedAssemblies()
                .Where(s => ExcludeAssembly(s.Name))
                .Select(s => LoadAssembly(s))
                .Where(s => s != null))
            {
                if (!assemblies.Contains(asb))
                {
                    assemblies.Add(asb);
                }

                FindReferenceAssemblies(asb, assemblies);
            }
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
                    services.AddSingleton(singReg.ServiceType, reg.Resolve());
                }
                else if (reg.GetType().IsGenericType && reg.GetType().GetGenericTypeDefinition() == typeof(TransientRegistration<,>))
                {
                    var types = reg.GetType().GetGenericArguments();
                    services.AddTransient(types[0], svr => reg.Resolve());
                }
                else if (reg.GetType().IsGenericType && reg.GetType().GetGenericTypeDefinition() == typeof(FuncRegistration<>))
                {
                    var types = reg.GetType().GetGenericArguments();
                    services.AddSingleton(types[0], reg.Resolve());
                }
            }

            return services;
        }

        private static bool ExcludeAssembly(string assemblyName)
        {
            return !assemblyName.StartsWith("system.", StringComparison.OrdinalIgnoreCase) &&
                    !assemblyName.StartsWith("microsoft.", StringComparison.OrdinalIgnoreCase);
        }

        private static Assembly LoadAssembly(AssemblyName assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch
            {
                return null;
            }
        }
    }

    internal class ConfigurationBinder
    {
        internal static void Bind(IServiceCollection services, IConfiguration configuration)
        {
            ConfigurationUnity.Bind<LoggingConfigurationSection>(configuration);
            ConfigurationUnity.Bind<CachingConfigurationSection>(configuration);
            ConfigurationUnity.Bind<ContainerConfigurationSection>(configuration);

            services.AddSingleton(typeof(ILogger), s => LoggerFactory.CreateLogger());
            services.AddSingleton(typeof(ICacheManager), s => CacheManagerFactory.CreateManager());
        }
    }
}
#endif