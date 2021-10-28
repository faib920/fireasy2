// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Caching;
using Fireasy.Common.Subscribes;
using Fireasy.Common.Threading;
using Fireasy.Redis;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 Redis 缓存管理组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddRedisCaching(this IServiceCollection services, Action<RedisCachingOptions> setupAction = null)
        {
            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services.AddSingleton<IDistributedCacheManager, CacheManager>()
                .AddSingleton<ICacheManager>(sp => sp.GetRequiredService<IDistributedCacheManager>());
        }

        /// <summary>
        /// 添加 Redis 消息订阅通知组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddRedisSubscriber(this IServiceCollection services, Action<RedisSubscribeOptions> setupAction = null)
        {
            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            services.AddSingleton<ISubscribeManager, SubscribeManager>();
            return services;
        }

        /// <summary>
        /// 添加 Redis 分布式事务锁组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddRedisDistributedLocker(this IServiceCollection services, Action<RedisDistributedLockerOptions> setupAction = null)
        {
            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            services.AddSingleton<IDistributedLocker, RedisLocker>();
            return services;
        }
    }
}
#endif