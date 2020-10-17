// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;

namespace Fireasy.Common.Options
{
    public static class OptionsHelper
    {
        /// <summary>
        /// 判断选项是否配置过，即判断是否通过 <see cref="IServiceCollection"/> 的 Configure 方法配置过。
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool IsConfigured(this IConfiguredOptions options)
        {
            return options != null && options.IsConfigured;
        }
    }
}
#endif
