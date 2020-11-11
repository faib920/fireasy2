// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Mapper;
using System;
using Fireasy.Mapster;
using Mapster;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 Mapster 对象映射组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddMapster(this IServiceCollection services, Action<MapsterOptions> setupAction)
        {
            var options = new MapsterOptions();
            setupAction?.Invoke(options);

            services.AddSingleton<IObjectMapper, ObjectMapper>();

            return services;
        }
    }
}
#endif