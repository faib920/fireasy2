// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETCOREAPP
using Fireasy.Web.EasyUI;
using Fireasy.Web.EasyUI.Binders;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MvcCoreMvcBuilderExtensions
    {
        /// <summary>
        /// 配置 EasyUI 的相关扩展。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IMvcBuilder ConfigureEasyUI(this IMvcBuilder builder, Action<EasyUIOptions> setupAction = null)
        {
            SettingsBindManager.RegisterBinder("validatebox", new ValidateBoxSettingBinder());
            SettingsBindManager.RegisterBinder("numberbox", new NumberBoxSettingBinder());

            var options = new EasyUIOptions();

            if (setupAction != null)
            {
                setupAction(options);

                foreach (var binder in options.binders)
                {
                    SettingsBindManager.RegisterBinder(binder.Key, binder.Value);
                }
            }

            if (setupAction != null)
            {
                builder.Services.Configure(setupAction);
            }

            return builder;
        }
    }
}
#endif