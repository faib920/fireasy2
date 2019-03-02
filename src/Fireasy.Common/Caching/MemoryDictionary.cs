// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.ComponentModel;
using System;
using System.Linq;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 缓存字典。无法继承此类。
    /// </summary>
    public sealed class MemoryDictionary : SafetyDictionary<string, CacheItem>
    {
        /// <summary>
        /// 检查字典的容量。
        /// </summary>
        /// <param name="cacheKey">缓存键。</param>
        /// <param name="capacity">设定的容量值。</param>
        /// <param name="onRemoved">移除的通知方法。</param>
        public void CheckCapacity(string cacheKey, int capacity, Action<CacheItem> onRemoved)
        {
            short gen = 0;
            while (Count > capacity && gen < CacheOptimizer.MAX_GEN_LIMIT)
            {
                //查找出未创建值的，以及最小代或过期的，将它移除
                var needRemoveds = LazyValues.Where(s => s.Key != cacheKey &&
                    (!s.Value.IsValueCreated || s.Value.Value.Gen == gen || s.Value.Value.HasExpired())).ToArray();

                foreach (var item in needRemoveds)
                {
                    if (!item.Value.IsValueCreated)
                    {
                        LazyValues.TryRemove(item.Key, out Lazy<CacheItem> entry);
                    }
                    else if (TryRemove(item.Key, out CacheItem entry))
                    {
                        onRemoved(entry);
                    }
                }

                gen++;
            }
        }
    }
}
