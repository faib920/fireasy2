// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Logging;
using Fireasy.NLog;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 NLog 日志管理组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddNLogger(this IServiceCollection services, Action<NLogOptions> setupAction = null)
        {
            var options = new NLogOptions();
            setupAction?.Invoke(options);
            services.AddSingleton(typeof(ILogger), p => new Logger(null, options));
            return services;
        }
    }
}
#endif