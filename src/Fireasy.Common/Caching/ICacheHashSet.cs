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
    /// 一个哈希集合管理器。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface ICacheHashSet<TKey, TValue>
    {
        /// <summary>
        /// 尝试从集合中获取指定 <paramref name="key"/> 的数据，如果没有则使用工厂函数添加对象到集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="valueCreator">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        TValue TryGet(TKey key, Func<TValue> valueCreator, Func<ICacheItemExpiration> expiration = null);

        /// <summary>
        /// 异步的，尝试从集合中获取指定 <paramref name="key"/> 的数据，如果没有则使用工厂函数添加对象到集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="valueCreator">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<TValue> TryGetAsync(TKey key, Func<Task<TValue>> valueCreator, Func<ICacheItemExpiration> expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 将值插入到哈希集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="value"></param>
        /// <param name="expiration">判断对象过期的对象。</param>
        void Add(TKey key, TValue value, ICacheItemExpiration expiration = null);

        /// <summary>
        /// 异步的，将值插入到哈希集合中。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="value"></param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task AddAsync(TKey key, TValue value, ICacheItemExpiration expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 尝试从集合中获取指定 <paramref name="key"/> 的数据。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGet(TKey key, out TValue value);

        /// <summary>
        /// 获取所有的 key。
        /// </summary>
        /// <returns></returns>
        IEnumerable<TKey> GetKeys();

        /// <summary>
        /// 获取所有的 value。
        /// </summary>
        /// <returns></returns>
        IEnumerable<TValue> GetValues();

        /// <summary>
        /// 异步的，获取所有的 key。
        /// </summary>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<IEnumerable<TKey>> GetKeysAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 从集合中移除指定键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        void Remove(TKey key);

        /// <summary>
        /// 异步的，从集合中移除指定键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task RemoveAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 确定集合中是否包含指定的键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <returns></returns>
        bool Contains(TKey key);

        /// <summary>
        /// 异步的，确定集合中是否包含指定的键的值。
        /// </summary>
        /// <param name="key">标识数据的 key。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<bool> ContainsAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取集合中对象的个数。
        /// </summary>
        long Count { get; }

        /// <summary>
        /// 清空整个集合。
        /// </summary>
        void Clear();

        /// <summary>
        /// 异步的，清空整个集合。
        /// </summary>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
