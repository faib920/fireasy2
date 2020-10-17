// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Subscribes;
using System;
using Fireasy.RabbitMQ;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加阿里云 AMQP 消息订阅通知组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddAliyunAMQP(this IServiceCollection services, Action<RabbitOptions> setupAction)
        {
            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            services.AddSingleton<ISubscribeManager, Fireasy.Aliyun.AMQP.SubscribeManager>();
            return services;
        }
    }
}
#endif