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
        private long discarded;
        private long current;
        private Timer timer;

        /// <summary>
        /// 初始化 <see cref="CacheOptimizer"/> 类的新实例。
        /// </summary>
        /// <param name="checkExpired">检查缓存过期的方法。</param>
        public CacheOptimizer(Action checkExpired)
        {
            timer = new Timer(state => checkExpired(), null, 1000 * 60, 1000 * 60 * 5);
        }

        /// <summary>
        /// 获取或设置要被丢弃的代。
        /// </summary>
        public long Discarded
        {
            get { return discarded; }
            set { discarded = value; }
        }

        /// <summary>
        /// 获取或设置当前代。
        /// </summary>
        public long Current
        {
            get { return current; }
            set { current = value; }
        }

        /// <summary>
        /// 返回丢弃的代数，并置标志递增。
        /// </summary>
        /// <returns></returns>
        public long Discard()
        {
            return Interlocked.Increment(ref discarded);
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

            item.Gen = Interlocked.Increment(ref current);

            return item;
        }
    }
}
