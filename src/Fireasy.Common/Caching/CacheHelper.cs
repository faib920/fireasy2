// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;

namespace Fireasy.Common.Caching
{
    public static class CacheHelper
    {
        /// <summary>
        /// 尝试通过 <see cref="ICacheKeyNormalizer"/> 来标准化缓存键。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="cacheKey"></param>
        /// <param name="specialKey"></param>
        /// <returns></returns>
        public static string GetCacheKey(this IServiceProvider serviceProvider, string cacheKey, object specialKey = null)
        {
            var normalizer = serviceProvider.TryGetService<ICacheKeyNormalizer>();
            return normalizer != null ? normalizer.NormalizeKey(cacheKey, specialKey) : cacheKey;
        }
    }
}
