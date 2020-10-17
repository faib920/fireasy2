// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 提供对缓存键标准化的接口。
    /// </summary>
    public interface ICacheKeyNormalizer
    {
        /// <summary>
        /// 标准化缓存键。
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="specialKey"></param>
        /// <returns></returns>
        string NormalizeKey(string cacheKey, object specialKey = null);
    }
}