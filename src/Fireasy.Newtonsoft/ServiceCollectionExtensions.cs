// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Serialization;
using System;
using Newton = Newtonsoft.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 newtonsoft 序列化组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddNewtonsoftSerializer(this IServiceCollection services, Action<Newton.JsonSerializerSettings> setupAction = null)
        {
            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            services.AddSingleton<ISerializer, Fireasy.Newtonsoft.JsonSerializer>();
            return services;
        }
    }
}
#endif