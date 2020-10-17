// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 缓存字典。无法继承此类。
    /// </summary>
    internal sealed class MemoryDictionary : ConcurrentDictionary<string, CacheItem>
    {
        /// <summary>
        /// 检查字典的容量。
        /// </summary>
        /// <param name="cacheKey">缓存键。</param>
        /// <param name="capacity">设定的容量值。</param>
        /// <param name="maxGen">当前最大的代。</param>
        /// <param name="onRemoved">移除的通知方法。</param>
        public void CheckCapacity(string cacheKey, int capacity, int maxGen, Action<CacheItem> onRemoved)
        {
            short gen = 0;
            while (Count > capacity && gen <= maxGen && gen < short.MaxValue)
            {
                //查找出未创建值的，以及最小代或过期的，将它移除
                var needRemoveds = this.Where(s => s.Key != cacheKey &&
                    (s.Value.Gen == gen || s.Value.HasExpired())).ToArray();

                foreach (var item in needRemoveds)
                {
                    if (TryRemove(item.Key, out CacheItem entry))
                    {
                        onRemoved(entry);
                    }
                }

                gen++;
            }
        }

        internal async Task<CacheItem> GetOrAddAsync(string cacheKey, Func<string, Task<CacheItem>> creator)
        {
            if (TryGetValue(cacheKey, out CacheItem item))
            {
                return item;
            }
            else
            {
                var value = await creator(cacheKey);
                TryAdd(cacheKey, value);
                return value;
            }
        }
    }
}
