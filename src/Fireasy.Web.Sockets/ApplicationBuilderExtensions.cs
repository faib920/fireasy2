// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD2_0
using Fireasy.Web.Sockets;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// 使用 WebSocket 中间件。
        /// </summary>
        /// <param name="app"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseWebSockets(this IApplicationBuilder app, Action<WebSocketBuildOption> setupAction)
        {
            var options = new WebSocketBuildOption();
            setupAction?.Invoke(options);

            return app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = options.KeepAliveInterval, ReceiveBufferSize = options.ReceiveBufferSize })
                .UseMiddleware<WebSocketMiddleware>(app.ApplicationServices, options);
        }
    }
}
#endif
