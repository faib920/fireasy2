// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 缓存管理的提供接口。
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// 将对象插入到缓存管理器中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="value">要插入到缓存的对象。</param>
        /// <param name="expire">对象存放于缓存中的有效时间，到期后将从缓存中移除。如果此值为 null，则默认有效时间为 30 分钟。</param>
        /// <param name="removeCallback">当对象从缓存中移除时，使用该回调方法通知应用程序。</param>
        T Add<T>(string cacheKey, T value, TimeSpan? expire = null, CacheItemRemovedCallback removeCallback = null);

        /// <summary>
        /// 异步的，将对象插入到缓存管理器中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="value">要插入到缓存的对象。</param>
        /// <param name="expire">对象存放于缓存中的有效时间，到期后将从缓存中移除。如果此值为 null，则默认有效时间为 30 分钟。</param>
        /// <param name="removeCallback">当对象从缓存中移除时，使用该回调方法通知应用程序。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task<T> AddAsync<T>(string cacheKey, T value, TimeSpan? expire = null, CacheItemRemovedCallback removeCallback = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 将对象插入到缓存管理器中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="value">要插入到缓存的对象。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="removeCallback">当对象从缓存中移除时，使用该回调方法通知应用程序。</param>
        T Add<T>(string cacheKey, T value, ICacheItemExpiration expiration, CacheItemRemovedCallback removeCallback = null);

        /// <summary>
        /// 异步的，将对象插入到缓存管理器中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="value">要插入到缓存的对象。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="removeCallback">当对象从缓存中移除时，使用该回调方法通知应用程序。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task<T> AddAsync<T>(string cacheKey, T value, ICacheItemExpiration expiration, CacheItemRemovedCallback removeCallback = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 确定缓存中是否包含指定的缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>如果缓存中包含指定缓存键的对象，则为 true，否则为 false。</returns>
        bool Contains(string cacheKey);

        /// <summary>
        /// 异步的，确定缓存中是否包含指定的缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果缓存中包含指定缓存键的对象，则为 true，否则为 false。</returns>
        Task<bool> ContainsAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取缓存中指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        object Get(string cacheKey);

        /// <summary>
        /// 异步的，获取缓存中指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        Task<object> GetAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取缓存中指定缓存键的对象。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        T Get<T>(string cacheKey);

        /// <summary>
        /// 异步的，取缓存中指定缓存键的对象。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        Task<T> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns></returns>
        TimeSpan? GetExpirationTime(string cacheKey);

        /// <summary>
        /// 异步的，获取缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<TimeSpan?> GetExpirationTimeAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 设置缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        void SetExpirationTime(string cacheKey, Func<ICacheItemExpiration> expiration);

        /// <summary>
        /// 异步的，设置缓存的有效时间。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task SetExpirationTimeAsync(string cacheKey, Func<ICacheItemExpiration> expiration, CancellationToken cancellationToken = default);

        /// <summary>
        /// 尝试获取指定缓存键的对象，如果没有则使用工厂函数添加对象到缓存中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        T TryGet<T>(string cacheKey, Func<T> factory, Func<ICacheItemExpiration> expiration = null);

        /// <summary>
        /// 异步的，尝试获取指定缓存键的对象，如果没有则使用工厂函数添加对象到缓存中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<T> TryGetAsync<T>(string cacheKey, Func<T> factory, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 尝试获取指定缓存键的对象，如果没有则使用工厂函数添加对象到缓存中。
        /// </summary>
        /// <param name="dataType">数据类型。</param>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        object TryGet(Type dataType, string cacheKey, Func<object> factory, Func<ICacheItemExpiration> expiration = null);

        /// <summary>
        /// 异步的，尝试获取指定缓存键的对象，如果没有则使用工厂函数添加对象到缓存中。
        /// </summary>
        /// <param name="dataType">数据类型。</param>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<object> TryGetAsync(Type dataType, string cacheKey, Func<object> factory, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 尝试获取指定缓存键的对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGet<T>(string cacheKey, out T value);

        /// <summary>
        /// 从缓存中移除指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        void Remove(string cacheKey);

        /// <summary>
        /// 异步的，从缓存中移除指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取所有的 key。
        /// </summary>
        /// <param name="pattern">匹配字符串。</param>
        /// <returns></returns>
        IEnumerable<string> GetKeys(string pattern);

        /// <summary>
        /// 异步的，获取所有的 key。
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetKeysAsync(string pattern, CancellationToken cancellationToken = default);

        /// <summary>
        /// 清除所有缓存。
        /// </summary>
        void Clear();

        /// <summary>
        /// 异步的，清除所有缓存。
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// </summary>
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
