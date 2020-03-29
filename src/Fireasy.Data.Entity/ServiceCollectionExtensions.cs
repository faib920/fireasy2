// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Reflection;
using Fireasy.Data.Entity;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Linq.Translators.Configuration;
using Fireasy.Data.Entity.Query;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

[assembly: ConfigurationBinder(typeof(Microsoft.Extensions.DependencyInjection.ConfigurationBinder))]

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 从现有的 <see cref="IServiceCollection"/> 里添加 <see cref="EntityContext"/> 对象。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddEntityContext(this IServiceCollection services, Action<EntityContextOptionsBuilder> setupAction = null)
        {
            services.RegisterBase();

            if (services is ServiceCollection coll)
            {
                foreach (var desc in coll.Where(s => typeof(EntityContext).IsAssignableFrom(s.ServiceType)).ToArray())
                {
                    services.Remove(desc);

                    var optionsType = ReflectionCache.GetMember("EntityOptionsType", desc.ServiceType, k => typeof(EntityContextOptions<>).MakeGenericType(k));
                    services.AddScoped(optionsType, sp => ContextOptionsFactory(desc.ServiceType, optionsType, sp, setupAction));
                    services.AddScoped(typeof(EntityContextOptions), sp => sp.GetRequiredService(optionsType));
                    services.AddScoped(desc.ServiceType, desc.ServiceType);
                }
            }

            return services;
        }

        /// <summary>
        /// 使 <see cref="IServiceCollection"/> 能够使用 Fireasy 中的 <see cref="EntityContext"/> 对象。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddEntityContext<TContext>(this IServiceCollection services, Action<EntityContextOptionsBuilder> setupAction = null) where TContext : EntityContext
        {
            services.RegisterBase();

            services.AddScoped(typeof(EntityContextOptions<TContext>), sp => ContextOptionsFactory<TContext>(sp, setupAction));
            services.AddScoped(typeof(EntityContextOptions), sp => sp.GetRequiredService<EntityContextOptions<TContext>>());
            services.AddScoped(typeof(TContext), typeof(TContext));

            return services;
        }

        /// <summary>
        /// 使 <see cref="IServiceCollection"/> 能够使用 Fireasy 缓冲池中的 <see cref="EntityContext"/> 对象。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <param name="maxSize">缓冲池的最大数量。</param>
        /// <returns></returns>
        public static IServiceCollection AddEntityContextPool<TContext>(this IServiceCollection services, Action<EntityContextOptionsBuilder> setupAction = null, int maxSize = 64) where TContext : EntityContext
        {
            services.RegisterBase();

            services.AddSingleton(typeof(EntityContextOptions<TContext>), sp => ContextOptionsFactory<TContext>(sp, setupAction));
            services.AddSingleton(typeof(EntityContextOptions), sp => sp.GetRequiredService<EntityContextOptions<TContext>>());
            services.AddSingleton(sp => new EntityContextPool<TContext>(sp, sp.GetRequiredService<EntityContextOptions<TContext>>(), maxSize));
            services.AddScoped<EntityContextPool<TContext>.Lease>();
            services.AddScoped(sp => sp.GetService<EntityContextPool<TContext>.Lease>().Context);

            return services;
        }

        private static EntityContextOptions ContextOptionsFactory(Type contextType, Type optionsType, IServiceProvider serviceProvider, Action<EntityContextOptionsBuilder> setupAction)
        {
            var builder = new EntityContextOptionsBuilder(contextType, optionsType.New<EntityContextOptions>(serviceProvider));
            setupAction?.Invoke(builder);
            return builder.Options;
        }

        private static EntityContextOptions ContextOptionsFactory<TContext>(IServiceProvider serviceProvider, Action<EntityContextOptionsBuilder> setupAction) where TContext : EntityContext
        {
            var builder = new EntityContextOptionsBuilder(typeof(TContext), new EntityContextOptions<TContext>(serviceProvider));
            setupAction?.Invoke(builder);
            return builder.Options;
        }

        private static IServiceCollection RegisterBase(this IServiceCollection services)
        {
            services.AddSingleton<IQueryCache, DefaultQueryCache>();
            services.AddSingleton<IExecuteCache, DefaultExecuteCache>();
            return services;
        }
    }

    internal class ConfigurationBinder
    {
        internal static void Bind(IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                ConfigurationUnity.Bind<TranslatorConfigurationSection>(configuration);
            }
            catch (Exception exp)
            {
                Tracer.Error($"{typeof(ConfigurationBinder).FullName} throw exception when binding:{exp.Output()}");
            }
        }
    }
}
#endif