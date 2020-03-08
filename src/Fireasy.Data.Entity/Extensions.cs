// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Provider;

namespace Fireasy.Data.Entity
{
    public static class IdentificationExtensions
    {
        /// <summary>
        /// 尝试从 <see cref="IInstanceIdentification"/> 实例中获取 <see cref="IProviderService"/> 实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identification"></param>
        /// <returns></returns>
        public static T GetProviderService<T>(this IInstanceIdentification identification) where T : class, IProviderService
        {
            return identification.ServiceProvider.TryGetService(() => identification.Provider.GetService<T>());
        }
    }
}