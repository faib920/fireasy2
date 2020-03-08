// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Logging;
using Fireasy.Log4net;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 log4net 日志管理组件。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddLog4netLogger(this IServiceCollection services, Action<Log4netOptions> setupAction = null)
        {
            var options = new Log4netOptions();
            setupAction?.Invoke(options);
            services.AddSingleton(typeof(ILogger), p => new Logger(null, options));
            return services;
        }
    }
}
#endif