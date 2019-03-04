// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.Language;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MvcCoreMvcBuilderExtensions
    {
        /// <summary>
        /// 配置 Fireasy 的 MVC 相关事项。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IMvcBuilder ConfigureFireasyMvc(this IMvcBuilder builder, Action<Fireasy.Web.Mvc.MvcOptions> setupAction = null)
        {
            var options = new Fireasy.Web.Mvc.MvcOptions();
            setupAction?.Invoke(options);



            if (options.UseTypicalJsonSerializer)
            {
                builder.Services.Configure<MvcOptions>(s => s.OutputFormatters.Insert(0, new Fireasy.Web.Mvc.JsonOutputFormatter(options)));
                builder.Services.AddSingleton<JsonResultExecutor, Fireasy.Web.Mvc.JsonResultExecutor>();
            }

            if (options.DisableModelValidator)
            {
                builder.Services.AddSingleton<IObjectModelValidator, Fireasy.Web.Mvc.NoneObjectModelValidator>();
            }

            if (options.UseReferenceAssembly)
            {
                builder.PartManager.FeatureProviders.Remove(builder.PartManager.FeatureProviders.First(f => f is MetadataReferenceFeatureProvider));
                builder.PartManager.FeatureProviders.Add(new Fireasy.Web.Mvc.ReferencesMetadataReferenceFeatureProvider());
            }

            if (options.UseRootRazorProject)
            {
                builder.Services.AddSingleton<RazorProjectFileSystem, Fireasy.Web.Mvc.BasedRazorProject>();
            }

            if (options.UseErrorHandleFilter)
            {
                builder.Services.Configure<MvcOptions>(s =>
                    {
                        s.Filters.Add(new Fireasy.Web.Mvc.HandleErrorAttribute());
                    });
            }

            if (options.UseJsonModelBinder)
            {
                builder.Services.Configure<MvcOptions>(s =>
                    {
                        s.ModelBinderProviders.Insert(0, new Fireasy.Web.Mvc.JsonModelBinderProvider(options));
                    });
            }

            if (setupAction != null)
            {
                builder.Services.Configure(setupAction);
            }

            return builder;
        }

        /// <summary>
        /// 添加 Session 复活通知类。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSessionRevive<T>(this IServiceCollection services) where T : Fireasy.Web.Mvc.ISessionReviveNotification
        {
            return services.AddTransient(typeof(Fireasy.Web.Mvc.ISessionReviveNotification), typeof(T));
        }

        /// <summary>
        /// Session 复活中间件。
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSessionRevive(this IApplicationBuilder app)
        {
            return app.UseMiddleware<Fireasy.Web.Mvc.SessionReviveMiddleware>();
        }
    }
}
#endif