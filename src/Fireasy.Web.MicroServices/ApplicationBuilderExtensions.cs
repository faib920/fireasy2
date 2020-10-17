// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Web.MicroServices;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// 使用微服务中间件。
        /// </summary>
        /// <param name="app"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMicroServices(this IApplicationBuilder app, Action<MicroServiceOption> setupAction)
        {
            var options = new MicroServiceOption();
            setupAction?.Invoke(options);

            return app.UseMiddleware<MicroServiceMiddleware>(app.ApplicationServices, options);
        }
    }
}
