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
    /// 通过该上下文，可以对本地的缓存使用前缀码进行拆分。
    /// </summary>
    public sealed class NativeCacheKeyContext : Scope<NativeCacheKeyContext>
    {
        /// <summary>
        /// 初始化 <see cref="NativeCacheKeyContext"/> 类的新实例。
        /// </summary>
        /// <param name="prefixKey">前缀码。</param>
        public NativeCacheKeyContext(string prefixKey)
        {
            PrefixKey = prefixKey;
        }

        /// <summary>
        /// 获取或设置前缀码。
        /// </summary>
        public string PrefixKey { get; set; }

        /// <summary>
        /// 尝试使用当前线程内的上下文进行缓存键的格式化。
        /// </summary>
        /// <param name="key">指定的缓存键。</param>
        /// <returns>如果当前线程内有此上下文，则返回 PrefixKey:<paramref name="key"/>的组合。</returns>
        public static string GetKey(string key)
        {
            return Current == null ? key : $"{Current.PrefixKey}:{key}";
        }
    }
}
