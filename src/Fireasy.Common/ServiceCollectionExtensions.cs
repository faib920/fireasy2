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
using Fireasy.Common.Mapper;
using Fireasy.Common.Mapper.Configuration;
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
using Microsoft.Extensions.Hosting;
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
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddFireasy(this IServiceCollection services, IConfiguration configuration, Action<CoreOptions> setupAction = null)
        {
            Guard.ArgumentNull(services, nameof(services));
            Guard.ArgumentNull(configuration, nameof(configuration));

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
            Guard.ArgumentNull(services, nameof(services));

            var section = ConfigurationUnity.GetSection<ContainerConfigurationSection>();
            if (section == null)
            {
                return services;
            }

            var setting = string.IsNullOrEmpty(containerName) ? section.Default : section.Settings[containerName];
            if (setting == null)
            {
                return services;
            }

            foreach (var reg in setting.Registrations)
            {
                if (reg.Assembly != null)
                {
                    Helpers.DiscoverServices(reg.Assembly, (svrType, implType, lifetime) => Register(services, svrType, implType, GetLifetime(lifetime)));
                }
                else
                {
                    Register(services, reg.ServiceType, reg.ImplementationType, GetLifetime(reg.Lifetime));
                }
            }

            return services;
        }

        /// <summary>
        /// 在 <see cref="IServiceCollection"/> 上注册程序集 <paramref name="assembly"/> 中可发现的服务。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly">程序集。</param>
        /// <returns></returns>
        public static IServiceCollection AddServices(this IServiceCollection services, Assembly assembly)
        {
            Guard.ArgumentNull(services, nameof(services));
            Guard.ArgumentNull(assembly, nameof(assembly));

            Helpers.DiscoverServices(assembly, (svrType, implType, lifetime) => Register(services, svrType, implType, GetLifetime(lifetime)));

            return services;
        }

        /// <summary>
        /// 在 <paramref name="services"/> 或指定的程序集中发现 <see cref="ISubscriber{TSubject}"/> 及 <see cref="ISubscribeHandler"/> 的类型，并将它们注册为订阅器。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly">指定的程序集。如果忽略该参数，则从 <paramref name="services"/> 里筛选。</param>
        /// <returns></returns>
        public static IServiceCollection AddSubscribers(this IServiceCollection services, Assembly assembly = null)
        {
            Guard.ArgumentNull(services, nameof(services));

            IEnumerable<Type> types = null;
            if (assembly == null)
            {
                types = Helper.DiscoveryTypes(services);
            }
            else
            {
                types = Helper.DiscoveryTypes(assembly);
                types.ForEach(s => services.TryAddScoped(s));
            }

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IHostedService, SubscribeHostedService>(sp => new SubscribeHostedService(sp, () => types)));

            return services;
        }

        /// <summary>
        /// 绑定所有和 fireasy 有关的配置项。
        /// </summary>
        /// <param name="callAssembly"></param>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        /// <param name="filter">过滤函数，如果指定了此函数，则只检索满足条件的程序集。</param>
        public static void Initialize(this IConfiguration configuration, Assembly callAssembly, IServiceCollection services = null, Func<Assembly, bool> filter = null)
        {
            callAssembly.ForEachAssemblies(ass =>
                {
                    var binderAttr = ass.GetCustomAttributes<ConfigurationBinderAttribute>().FirstOrDefault();
                    var type = binderAttr != null ? binderAttr.BinderType : ass.GetType("Microsoft.Extensions.DependencyInjection.ConfigurationBinder", false);
                    if (type != null)
                    {
                        var method = type.GetMethod("Bind", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new[] { typeof(IServiceCollection), typeof(IConfiguration) }, null);
                        if (method != null)
                        {
                            method.Invoke(null, new object[] { services, configuration });
                        }
                    }
                }, filter ?? FilterAssembly);
        }

        private static bool FilterAssembly(Assembly assembly)
        {
            return assembly.IsDefined(typeof(ConfigurationBinderAttribute));
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

        private static void Register(IServiceCollection services, Type svrType, Type implType, ServiceLifetime lifetime)
        {
            if (implType == null)
            {
                Tracer.Debug($"Couldn't find the implementation of '{svrType}'.");
            }
            else if (svrType != null)
            {
                if (typeof(IRepeatableService).IsAssignableFrom(implType))
                {
                    services.TryAddEnumerable(ServiceDescriptor.Describe(svrType, CheckAopProxyType(implType), lifetime));
                }
                else
                {
                    services.Replace(ServiceDescriptor.Describe(svrType, CheckAopProxyType(implType), lifetime));
                }

                Tracer.Debug($"{lifetime}-Descriptor has been registered: {svrType} --> {implType}.");
            }
        }

        private static ServiceLifetime GetLifetime(Lifetime lifetime)
        {
            return lifetime switch
            {
                Lifetime.Transient => ServiceLifetime.Transient,
                Lifetime.Singleton => ServiceLifetime.Singleton,
                Lifetime.Scoped => ServiceLifetime.Scoped,
                _ => ServiceLifetime.Transient
            };
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
                ConfigurationUnity.Bind<ObjectMapperConfigurationSection>(configuration);
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
                    .AddTaskScheduler()
                    .AddMapper();
            }
        }
    }
}
#endif