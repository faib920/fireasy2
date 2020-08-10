// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using Fireasy.Common.ComponentModel;
using System;
using System.Threading;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 提供对缓存的优化。
    /// </summary>
    internal class CacheOptimizer : DisposableBase
    {
        private readonly Timer _timer;

        /// <summary>
        /// 获取当前最大的代。
        /// </summary>
        public int CurrentMaxGen { get; private set; }

        /// <summary>
        /// 初始化 <see cref="CacheOptimizer"/> 类的新实例。
        /// </summary>
        /// <param name="checkExpired">检查缓存过期的方法。</param>
        /// <param name="period">检查周期（以毫秒为单位）。</param>
        public CacheOptimizer(Action checkExpired, int period = 60000)
        {
            _timer = new Timer(state => checkExpired(), null, 3000, period);
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
                if (item.Gen < short.MaxValue)
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

        protected override bool Dispose(bool disposing)
        {
            _timer?.Dispose();

            return base.Dispose(disposing);
        }
    }
}
