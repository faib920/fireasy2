// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD2_0
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
            if (setupAction != null)
            {
                setupAction(options);

                Fireasy.Web.Mvc.GlobalSetting.Converters.AddRange(options.Converters);
            }

            if (options.UseCustomResultExecutor)
            {
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

            builder.Services.AddSingleton<RazorProject, Fireasy.Web.Mvc.BasedRazorProject>();

            builder.Services.Configure<MvcOptions>(s =>
                {
                    s.Filters.Add(new Fireasy.Web.Mvc.HandleErrorAttribute());
                });

            if (options.UseJsonModelBinder)
            {
                builder.Services.Configure<MvcOptions>(s =>
                    {
                        s.ModelBinderProviders.Insert(0, new Fireasy.Web.Mvc.JsonModelBinderProvider());
                    });
            }


            return builder;
        }
    }
}
#endif