// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common;
using Fireasy.Common.Aop;
using Fireasy.Common.Caching;
using Fireasy.Common.Caching.Configuration;
using Fireasy.Common.Composition.Configuration;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using Fireasy.Common.Ioc.Configuration;
using Fireasy.Common.Localization;
using Fireasy.Common.Localization.Configuration;
using Fireasy.Common.Logging;
using Fireasy.Common.Logging.Configuration;
using Fireasy.Common.Options;
using Fireasy.Common.Serialization;
using Fireasy.Common.Serialization.Configuration;
using Fireasy.Common.Subscribes;
using Fireasy.Common.Subscribes.Configuration;
using Fireasy.Common.Tasks;
using Fireasy.Common.Tasks.Configuration;
using Fireasy.Common.Threading;
using Fireasy.Common.Threading.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[assembly: ConfigurationBinder(typeof(Microsoft.Extensions.DependencyInjection.ConfigurationBinder))]

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 使 <see cref="IServiceCollection"/> 能够使用 Fireasy 框架中的配置。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddFireasy(this IServiceCollection services, IConfiguration configuration, Action<CoreOptions> setupAction = null)
        {
            var options = new CoreOptions();
            setupAction?.Invoke(options);

            Tracer.Disabled = configuration.GetSection("appSettings:DisableTracer").Value.To<bool>();

            configuration.Initialize(Assembly.GetCallingAssembly(), services, options.AssemblyFilter);

            services.AddTransient(typeof(IOptionsFactory<>), typeof(ConfiguredOptionsFactory<>));

            return services;
        }

        /// <summary>
        /// 在 <see cref="IServiceCollection"/> 上注册 Fireasy 容器里的定义。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="containerName">容器名称。</param>
        /// <returns></returns>
        public static IServiceCollection AddIoc(this IServiceCollection services, string containerName = null)
        {
            var section = ConfigurationUnity.GetSection<ContainerConfigurationSection>();
            var setting = string.IsNullOrEmpty(containerName) ? section.Default : section.Settings[containerName];
            if (setting == null)
            {
                return services;
            }

            foreach (var reg in setting.Registrations)
            {
                var lifetime = reg.Lifetime switch
                {
                    Lifetime.Transient => ServiceLifetime.Transient,
                    Lifetime.Singleton => ServiceLifetime.Singleton,
                    Lifetime.Scoped => ServiceLifetime.Scoped,
                    _ => ServiceLifetime.Transient
                };

                void register(IServiceCollection services, Type svrType, Type implType)
                {
                    if (implType == null)
                    {
                        Tracer.Debug($"Couldn't find the implementation of '{svrType}'.");
                    }
                    else if (svrType != null)
                    {
                        Tracer.Debug($"{lifetime}-Descriptor has been registered: {svrType} --> {implType}.");
                        services.Add(ServiceDescriptor.Describe(svrType, CheckAopProxyType(implType), lifetime));
                    }
                }

                if (reg.Assembly != null)
                {
                    Helpers.DiscoverAssembly(reg.Assembly, (svrType, implType) => register(services, svrType, implType));
                }
                else
                {
                    register(services, reg.ServiceType, reg.ImplementationType);
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

        /// <summary>
        /// 绑定所有和 fireasy 有关的配置项。
        /// </summary>
        /// <param name="callAssembly"></param>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        public static void Initialize(this IConfiguration configuration, Assembly callAssembly, IServiceCollection services = null, Func<AssemblyName, bool> filter = null)
        {
            var assemblies = new List<Assembly>();

            FindReferenceAssemblies(callAssembly, assemblies, filter);

            assemblies.ForEach(assembly =>
                {
                    var binderAttr = assembly.GetCustomAttributes<ConfigurationBinderAttribute>().FirstOrDefault();
                    var type = binderAttr != null ? binderAttr.BinderType : assembly.GetType("Microsoft.Extensions.DependencyInjection.ConfigurationBinder");
                    if (type != null)
                    {
                        var method = type.GetMethod("Bind", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(IServiceCollection), typeof(IConfiguration) }, null);
                        if (method != null)
                        {
                            method.Invoke(null, new object[] { services, configuration });
                        }
                    }
                });

            assemblies.Clear();
        }

        private static bool ExcludeAssembly(AssemblyName assemblyName)
        {
            return !assemblyName.Name.StartsWith("system.", StringComparison.OrdinalIgnoreCase) &&
                    !assemblyName.Name.StartsWith("microsoft.", StringComparison.OrdinalIgnoreCase);
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
        private static void FindReferenceAssemblies(Assembly assembly, List<Assembly> assemblies, Func<AssemblyName, bool> filter)
        {
            foreach (var asb in assembly.GetReferencedAssemblies()
                .Where(filter ?? ExcludeAssembly)
                .Select(s => LoadAssembly(s))
                .Where(s => s != null))
            {
                if (!assemblies.Contains(asb))
                {
                    assemblies.Add(asb);
                }

                FindReferenceAssemblies(asb, assemblies, filter);
            }
        }

    }

    internal class ConfigurationBinder
    {
        internal static void Bind(IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                ConfigurationUnity.Bind<LoggingConfigurationSection>(configuration);
                ConfigurationUnity.Bind<CachingConfigurationSection>(configuration);
                ConfigurationUnity.Bind<ContainerConfigurationSection>(configuration);
                ConfigurationUnity.Bind<LockerConfigurationSection>(configuration);
                ConfigurationUnity.Bind<SubscribeConfigurationSection>(configuration);
                ConfigurationUnity.Bind<ImportConfigurationSection>(configuration);
                ConfigurationUnity.Bind<SerializerConfigurationSection>(configuration);
                ConfigurationUnity.Bind<StringLocalizerConfigurationSection>(configuration);
                ConfigurationUnity.Bind<TaskScheduleConfigurationSection>(configuration);
            }
            catch (Exception exp)
            {
                Tracer.Error($"{typeof(ConfigurationBinder).FullName} throw exception when binding:{exp.Output()}");
            }

            if (services != null)
            {
                services.AddLogger()
                    .AddCaching()
                    .AddSubscriber()
                    .AddLocker()
                    .AddSerializer()
                    .AddStringLocalizer()
                    .AddTaskScheduler();
            }
        }
    }
}
#endif