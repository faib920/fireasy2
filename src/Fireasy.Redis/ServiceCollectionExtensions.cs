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
        public static IServiceCollection AddRedisCaching(this IServiceCollection services, Action<RedisCachingOptions> setupAction)
        {
            var options = new RedisCachingOptions();
            setupAction?.Invoke(options);
            services.AddSingleton(typeof(ICacheManager), p => new CacheManager(options));
            return services;
        }

        /// <summary>
        /// 添加 Redis 消息订阅通知组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddRedisSubscriber(this IServiceCollection services, Action<RedisSubscribeOptions> setupAction)
        {
            var options = new RedisSubscribeOptions();
            setupAction?.Invoke(options);
            services.AddSingleton(typeof(ISubscribeManager), p => new SubscribeManager(options));
            return services;
        }

        /// <summary>
        /// 添加 Redis 分布式事务锁组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddRedisDistributedLocker(this IServiceCollection services, Action<RedisDistributedLockerOptions> setupAction)
        {
            var options = new RedisDistributedLockerOptions();
            setupAction?.Invoke(options);
            services.AddSingleton(typeof(ISubscribeManager), p => new RedisLocker(options));
            return services;
        }
    }
}
#endif