// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Subscribes;
using Fireasy.RabbitMQ;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 RabbitMQ 消息订阅通知组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQSubscriber(this IServiceCollection services, Action<RabbitOptions> setupAction)
        {
            var options = new RabbitOptions();
            setupAction?.Invoke(options);
            services.AddSingleton(typeof(ISubscribeManager), p => new SubscribeManager(options));
            return services;
        }
    }
}
#endif