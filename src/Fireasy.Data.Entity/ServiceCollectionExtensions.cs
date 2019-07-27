// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Configuration;
using Fireasy.Data.Entity;
using Fireasy.Data.Entity.Linq.Translators.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 从现有的 <see cref="IServiceCollection"/> 里添加 <see cref="EntityContext"/> 对象。
        /// </summary>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static EntityContextOptionsBuilder AddEntityContext(this IServiceCollection services, Action<EntityContextOptions> setupAction = null)
        {
            var options = new EntityContextOptions();
            setupAction?.Invoke(options);

            var builder = new EntityContextOptionsBuilder(options);

            if (services is ServiceCollection coll)
            {
                var desc = coll.FirstOrDefault(s => typeof(EntityContext).IsAssignableFrom(s.ServiceType));
                if (desc != null)
                {
                    services.Remove(desc);

                    if (setupAction != null)
                    {
                        services.Configure(setupAction);
                    }

                    services.AddScoped(s => options);
                    services.AddScoped(desc.ServiceType);

                    return builder;
                }
            }

            return builder;
        }

        /// <summary>
        /// 使 <see cref="IServiceCollection"/> 能够使用 Fireasy 中的 <see cref="EntityContext"/> 对象。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static EntityContextOptionsBuilder AddEntityContext<TContext>(this IServiceCollection services, Action<EntityContextOptions> setupAction = null) where TContext : EntityContext
        {
            var options = new EntityContextOptions();
            setupAction?.Invoke(options);

            var builder = new EntityContextOptionsBuilder(options);

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            services.AddScoped(s => options);
            services.AddScoped<TContext>();

            return builder;
        }
    }

    internal class ConfigurationBinder
    {
        internal static void Bind(IServiceCollection services, IConfiguration configuration)
        {
            ConfigurationUnity.Bind<TranslatorConfigurationSection>(configuration);
        }
    }
}
#endif