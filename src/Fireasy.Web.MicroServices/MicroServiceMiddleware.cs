// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Fireasy.Web.MicroServices
{
    /// <summary>
    /// 微服务中间件。
    /// </summary>
    public class MicroServiceMiddleware
    {
        private RequestDelegate next;
        private MicroServiceOption option;
        private IServiceProvider serviceProvider;

        public MicroServiceMiddleware(RequestDelegate next, IServiceProvider serviceProvider, MicroServiceOption option)
        {
            this.next = next;
            this.serviceProvider = serviceProvider;
            this.option = option;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!string.IsNullOrEmpty(option.Path) && 
                context.Request.Path.Value.StartsWith(option.Path, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!ServiceManager.IsAuthenticated(context, option))
                {
                    context.Response.StatusCode = 401;
                    return;
                }

                await ServiceManager.ExecuteAsync(context, option);
            }
            else
            {
                await next(context);
            }
        }
    }
}
