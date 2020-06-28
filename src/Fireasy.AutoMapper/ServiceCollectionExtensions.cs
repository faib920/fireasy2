// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using AP = AutoMapper;
using Fireasy.Common.Mapper;
using System;
using Fireasy.AutoMapper;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 AutoMapper 对象映射组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<AutoMapperOptions> setupAction)
        {
            var options = new AutoMapperOptions();
            setupAction?.Invoke(options);

            var mapperConfiguration = new AP.MapperConfiguration(c =>
            {
                options.Configurators.ForEach(s => s?.Invoke(c));
            });

            services.AddSingleton(sp => mapperConfiguration.CreateMapper());
            services.AddSingleton<IObjectMapper, ObjectMapper>();

            return services;
        }
    }
}
#endif