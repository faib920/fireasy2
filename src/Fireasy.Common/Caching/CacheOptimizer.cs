// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Threading;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 提供对缓存的优化。
    /// </summary>
    public class CacheOptimizer
    {
        private readonly Timer timer;

        /// <summary>
        /// 最大代限制。
        /// </summary>
        public const short MAX_GEN_LIMIT = 30000;

        /// <summary>
        /// 获取当前最大的代。
        /// </summary>
        public int CurrentMaxGen { get; private set; }

        /// <summary>
        /// 初始化 <see cref="CacheOptimizer"/> 类的新实例。
        /// </summary>
        /// <param name="checkExpired">检查缓存过期的方法。</param>
        public CacheOptimizer(Action checkExpired)
        {
            //5分钟清理一次过期的缓存
            timer = new Timer(state => checkExpired(), null, 1000 * 60, 1000 * 60 * 5);
        }

        /// <summary>
        /// 缓存项递增代数。若指定 <paramref name="checkExpired"/> 则会在递增之前进行过期检查。
        /// </summary>
        /// <param name="item">缓存项。</param>
        /// <param name="checkExpired">是否查检过期。</param>
        /// <returns>如果过期，则返回 null。</returns>
        public CacheItem Update(CacheItem item, bool checkExpired = true)
        {
            if (checkExpired && item.HasExpired())
            {
                return null;
            }

            if (item.Gen != -1)
            {
                if (item.Gen < MAX_GEN_LIMIT)
                {
                    item.Gen++;
                }
                else
                {
                    item.Gen = 0;
                }
            }

            CurrentMaxGen = Math.Max(CurrentMaxGen, item.Gen);

            return item;
        }
    }
}
