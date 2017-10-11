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
    /// 当对象从缓存中移除时，该回调方法用于通知应用程序。
    /// </summary>
    /// <param name="key">被移除的缓存键。</param>
    /// <param name="value">被移除的缓存对象。</param>
    public delegate void CacheItemRemovedCallback(string key, object value);
}
